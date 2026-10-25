using CFB27.Data.Model;
using MaddenFranchise;

namespace StatusPage
{
    /// <summary>
    /// Reads the season's games straight from the save's records (no reflection-based transforms, so it publishes with
    /// Native AOT).
    /// </summary>
    internal static class SeasonReader
    {
        private const int NoTeamIndex = 255;
        private const int Top25 = 25;
        private const string NoRef = "00000000000000000000000000000000";

        private static readonly string[] MonthNames =
            { "", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        /// <summary>
        /// CFP quarterfinal and semifinal rows are bracket slots, not bowls ("CFP Quarterfinal"); the New Year's Six bowl
        /// hosting the game is only known from the game's stadium.
        /// </summary>
        private static readonly Dictionary<uint, string> NewYearsSixBowls = new Dictionary<uint, string>
        {
            { 0x806385CF, "Sugar Bowl" },
            { 0x80654CDE, "Rose Bowl" },
            { 0x80638637, "Peach Bowl" },
            { 0x80638607, "Orange Bowl" },
            { 0x80639DE7, "Fiesta Bowl" },
            { 0x806385D0, "Cotton Bowl" },
        };

        public static PageData Read(FranchiseFile dynasty, string saveName)
        {
            var seasonInfo = dynasty.GetPrimaryTable("SeasonInfo").Records.First(r => !r.IsEmpty);
            int championshipWeek = Convert.ToInt32(seasonInfo["RegularSeasonWeekConferenceChampionship"]);

            var teams = dynasty.GetPrimaryTable("Team", "DisplayName");
            teams.ReadRecords();
            var teamTableId = teams.Header.TableId;

            var bowls = dynasty.GetPrimaryTable("BowlGame");
            bowls.ReadRecords();

            var conferences = CFBConference.Read(dynasty);
            var conferenceByTeam = conferences
                .SelectMany(c => c.TeamRows.Select(row => (Row: row, c.Name)))
                .GroupBy(x => x.Row)
                .ToDictionary(g => g.Key, g => g.First().Name ?? string.Empty);

            // stadium names: the teams' home stadiums (several bowls and the title game are at one) and the neutral sites
            var stadiumNames = new Dictionary<uint, string>();
            foreach (var team in teams.Records.Where(t => !t.IsEmpty && (t["Stadium"] as string ?? NoRef) != NoRef))
            {
                var stadiumHome = CFBStadium.FindHome(team["AssetName"] as string, team["ShortName"] as string);
                if (stadiumHome != null)
                {
                    stadiumNames.TryAdd(Convert.ToUInt32(team["Stadium"] as string ?? NoRef, 2), stadiumHome.Name);
                }
            }

            foreach (var neutralSite in CFBStadium.NeutralSites)
            {
                stadiumNames.TryAdd(neutralSite.Key, neutralSite.Value);
            }

            // rivalry names by team pair (Rivalry.Team1 / Team2); the series record isn't shown since it may already count
            // the hidden week's games
            var rivalries = new Dictionary<(int, int), string>();
            var rivalryTable = dynasty.GetPrimaryTable("Rivalry");
            rivalryTable.ReadRecords();
            foreach (var rivalry in rivalryTable.Records.Where(r => !r.IsEmpty))
            {
                var team1 = rivalry.GetReferenceDataByKey("Team1");
                var team2 = rivalry.GetReferenceDataByKey("Team2");
                var name = rivalry["Name"] as string;
                if (team1?.TableId == teamTableId && team2?.TableId == teamTableId && !string.IsNullOrEmpty(name))
                {
                    rivalries.TryAdd(Pair(team1.Value.RowNumber, team2.Value.RowNumber), name);
                }
            }

            TeamInfo Team(int row)
            {
                var team = teams.Records[row];
                int rank = Convert.ToInt32(team["MediaPoll_CurrentRank"]);
                bool fcs = Convert.ToInt32(team["TeamIndex"]) == NoTeamIndex;
                return new TeamInfo
                {
                    Row = row,
                    Name = team["DisplayName"] as string ?? string.Empty,
                    Fcs = fcs,
                    Rank = !fcs && rank >= 1 && rank <= Top25 ? rank : 0,
                };
            }

            var games = new List<GameInfo>();
            var seasonGames = dynasty.GetPrimaryTable("SeasonGame");
            seasonGames.ReadRecords();
            foreach (var game in seasonGames.Records.Where(r => !r.IsEmpty))
            {
                // bowls and title games are on the schedule before their teams are known
                var home = game.GetReferenceDataByKey("HomeTeam");
                var away = game.GetReferenceDataByKey("AwayTeam");
                if (home?.TableId != teamTableId || away?.TableId != teamTableId)
                {
                    continue;
                }

                int homeRow = home.Value.RowNumber, awayRow = away.Value.RowNumber;
                var status = game["GameStatus"] as string;
                bool played = status == "HomeWon" || status == "AwayWon";
                int month = Convert.ToInt32(game["GameDateMonth"]), day = Convert.ToInt32(game["GameDateDay"]);

                var stadium = game["Stadium"] as string ?? NoRef;
                var homeStadium = teams.Records[homeRow]["Stadium"] as string ?? NoRef;
                bool neutral = stadium != NoRef && stadium != homeStadium;
                // a game without its own stadium (every home game, and the CFP first round) is at the home team's
                uint stadiumId = Convert.ToUInt32(stadium != NoRef ? stadium : homeStadium, 2);
                var bowl = BowlName(game, bowls, stadiumId);

                games.Add(new GameInfo
                {
                    Id = game.Index,
                    Week = Convert.ToInt32(game["SeasonWeek"]),
                    Date = $"{game["DayOfWeek"]}, {MonthNames[month]} {day}",
                    // the season runs August to January
                    SortKey = ((month < 7 ? month + 12 : month) * 100 + day) * 10000 + Convert.ToInt32(game["TimeOfDay"]),
                    Time = FormatTime(Convert.ToInt32(game["TimeOfDay"])),
                    Home = Team(homeRow),
                    Away = Team(awayRow),
                    Neutral = neutral,
                    // where it's played, for neutral sites and every bowl
                    Site = (neutral || bowl != null) && stadiumNames.TryGetValue(stadiumId, out var site) ? site : null,
                    Bowl = bowl,
                    Rivalry = rivalries.TryGetValue(Pair(homeRow, awayRow), out var rivalryName) ? rivalryName : null,
                    Played = played,
                    HomeScore = played ? Convert.ToInt32(game["HomeScore"]) : 0,
                    AwayScore = played ? Convert.ToInt32(game["AwayScore"]) : 0,
                    Overtime = played && Convert.ToBoolean(game["IsOvertimeGame"]),
                    Conferences = new[] { homeRow, awayRow }
                        .Select(row => conferenceByTeam.TryGetValue(row, out var name) ? name : null)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .Distinct()
                        .ToList()!,
                });
            }

            int latestWeek = games.Where(g => g.Played).Select(g => g.Week).DefaultIfEmpty(-1).Max();
            var weekTypes = seasonGames.Records.Where(r => !r.IsEmpty)
                .GroupBy(r => Convert.ToInt32(r["SeasonWeek"]))
                .ToDictionary(g => g.Key, g => g.First()["SeasonWeekType"] as string ?? string.Empty);

            return new PageData
            {
                Save = saveName,
                Season = Convert.ToInt32(seasonInfo["CurrentSeasonYear"]),
                LatestWeek = latestWeek,
                Weeks = games.Select(g => g.Week).Distinct().OrderBy(w => w)
                    .Select(w => new WeekInfo { Week = w, Label = WeekLabel(w, weekTypes[w], championshipWeek) })
                    .ToList(),
                Conferences = conferences.Select(c => c.Name ?? string.Empty).Where(n => n.Length > 0).ToList(),
                Games = games.OrderBy(g => g.SortKey).ToList(),
                Theme = TeamTheme.ForUserTeam(dynasty, teams),
            };
        }

        private static (int, int) Pair(int a, int b) => a < b ? (a, b) : (b, a);

        /// <summary>The bowl's name; a CFP quarterfinal or semifinal takes its New Year's Six name from the stadium.</summary>
        private static string? BowlName(FranchiseFileRecord game, FranchiseFileTable bowls, uint stadiumId)
        {
            var bowlRef = game.GetReferenceDataByKey("BowlGame");
            if (bowlRef == null || bowlRef.Value.TableId != bowls.Header.TableId)
            {
                return null;
            }

            var bowl = bowls.Records[bowlRef.Value.RowNumber];
            var name = bowl["Name"] as string ?? string.Empty;
            return NewYearsSixBowls.TryGetValue(stadiumId, out var host) && Convert.ToBoolean(bowl["IsPlayoffBowl"])
                ? $"{host} ({name})"
                : name;
        }

        private static string WeekLabel(int week, string weekType, int championshipWeek)
        {
            if (week == championshipWeek)
            {
                return "Championships";
            }

            switch (weekType)
            {
                case "BowlSeason1": return "Bowls 1";
                case "BowlSeason2": return "Bowls 2";
                case "BowlSeason3": return "Semifinals";
                case "NationalChampionship": return "Title Game";
                default: return $"Week {week}";
            }
        }

        /// <summary>TimeOfDay is minutes after midnight (ET).</summary>
        private static string FormatTime(int minutes)
        {
            if (minutes <= 0)
            {
                return "TBA";
            }

            int hour = minutes / 60 % 24, minute = minutes % 60;
            return $"{(hour % 12 == 0 ? 12 : hour % 12)}:{minute:00} {(hour < 12 ? "am" : "pm")}";
        }
    }
}

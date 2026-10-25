using MaddenFranchise;
using MaddenFranchise.Util;
using System.Linq;

namespace BowlEditor
{
    /// <summary>
    /// The bowl games whose teams can still be changed: the regular bowls of the first two bowl weeks, from the current
    /// week on. A matchup is only SeasonGame.HomeTeam / AwayTeam (nothing else in the save names the teams). The CFP
    /// games are listed but not editable: the game builds the bracket from the week-16 CFP poll (see <see cref="CfpPoll"/>).
    /// <para>
    /// Edit in bowl week 1 (week 17). The matchups seen at week 16 are only projections: advancing re-picks every bowl
    /// from the standings, tie-ins and CFP rankings, then simulates the new week's games straight away. Those results
    /// aren't posted (records, season stats) until the next advance; until then a game's stats hang off its
    /// Home/AwayTeamStatCache, Home/AwayPlayerStatCache, ScoringSummaries and InjuryCache. So an edited game that was
    /// already simulated is put back to Unplayed with those links cleared (the old result is never posted), and the next
    /// advance simulates it like the user's own unplayed game. Games that aren't changed keep their result. (MyDynasty
    /// wrote a final score instead, so the game never simulated its edited bowls and they kept no box score or stats.)
    /// </para>
    /// <para>
    /// The user's own game this week also carries the user's "play your game" request (a SeasonGameRequest with a
    /// TargetUser), which is tied to the game, not the team. Moving the user's team out of it leaves the request on a
    /// game the team isn't in, so the request is closed the way the game closes an answered one, and the vacated game
    /// gets a written final score, as MyDynasty does: the game only plays the user's game through that request, so left
    /// unplayed it would never be played (game-verified). Its two teams get the result but no box score.
    /// </para>
    /// <para>
    /// The CFP bracket is the BowlGame rows with IsPlayoffBowl (PlayoffBracketSlot 0-3 first round, 4-7 quarterfinals,
    /// 8-9 semifinals, 10 title game), whose Conference1Rank / Conference2Rank hold the 0-based seeds. Seeds 1-4 wait at
    /// home in quarterfinals 4-7 for the winner of first-round slot 0-3. Nothing stores which team holds a seed: the
    /// bracket screen seeds the field from the CFP rankings (Team.CFPPoll_CurrentRank) each time it opens.
    /// </para>
    /// </summary>
    internal sealed class BowlMatchups
    {
        private const int NoTeamIndex = 255;
        private const int FirstBowlWeek = 17;
        private const int LastEditableWeek = 18;
        private const int FirstQuarterfinalSlot = 4;
        private const string NoRef = "00000000000000000000000000000000";

        /// <summary>A simulated game's links to its not-yet-posted results.</summary>
        private static readonly string[] ResultLinks =
            { "HomeTeamStatCache", "AwayTeamStatCache", "HomePlayerStatCache", "AwayPlayerStatCache", "ScoringSummaries", "InjuryCache" };

        private static readonly string[] Scores =
        {
            "HomeScore", "AwayScore", "HomeScoreOT", "AwayScoreOT",
            "HomeScoreQuarter1", "HomeScoreQuarter2", "HomeScoreQuarter3", "HomeScoreQuarter4",
            "AwayScoreQuarter1", "AwayScoreQuarter2", "AwayScoreQuarter3", "AwayScoreQuarter4",
        };

        /// <summary>
        /// The grid's order: the CFP first round by seeds, the New Year's Six (the quarterfinal hosts), then the bowls.
        /// Names are matched ignoring case and punctuation (the save spells it "Hawaii Bowl"); any bowl not listed goes
        /// after these, by date.
        /// </summary>
        private static readonly string[] DisplayOrder =
        {
            "CFP 5 v 12", "CFP 6 v 11", "CFP 7 v 10", "CFP 8 v 9",
            "Rose Bowl", "Sugar Bowl", "Orange Bowl", "Cotton Bowl", "Peach Bowl", "Fiesta Bowl",
            "Citrus Bowl", "Alamo Bowl", "Reliaquest Bowl", "Pop-Tarts Bowl", "Las Vegas Bowl", "Gator Bowl",
            "Duke's Mayo Bowl", "Music City Bowl", "Texas Bowl", "Liberty Bowl", "Holiday Bowl", "Sun Bowl", "Rate Bowl",
            "Military Bowl", "Birmingham Bowl", "Independence Bowl", "Gasparilla Bowl", "First Responder Bowl",
            "Fenway Bowl", "Hawai'i Bowl", "Armed Forces Bowl", "New Orleans Bowl", "Boca Raton Bowl", "New Mexico Bowl",
            "Famous Idaho Potato Bowl", "Cure Bowl", "Myrtle Beach Bowl", "Arizona Bowl", "68 Ventures Bowl",
            "Salute to Veterans Bowl", "Xbox Bowl", "Frisco Bowl",
        };

        /// <summary>
        /// A CFP quarterfinal row is a bracket slot ("CFP Quarterfinal"); the New Year's Six bowl hosting it is only known
        /// from the game's stadium.
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

        private static readonly string[] MonthNames =
            { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        private readonly FranchiseFileTable teams;
        private readonly FranchiseFileTable seasonGames;
        private readonly FranchiseFileTable requests;
        private readonly int currentWeek;

        /// <summary>RequestId of a request that's no longer open (int.MaxValue - 100), as the game writes it.</summary>
        private const long ClosedRequestId = 2147483547;

        /// <summary>The user-controlled teams (Team.UserCharacter set, or the team of a user-controlled coach).</summary>
        public HashSet<int> UserTeams { get; } = new HashSet<int>();

        /// <summary>Whether the last <see cref="Apply"/> closed the user's play-your-game request.</summary>
        public bool ClosedUserRequest { get; private set; }

        /// <summary>The final scores the last <see cref="Apply"/> wrote into games the user's team left, e.g. "Independence Bowl: UNLV 17, Nebraska 42".</summary>
        public List<string> WrittenResults { get; } = new List<string>();

        public List<BowlSlot> Games { get; } = new List<BowlSlot>();

        /// <summary>FBS teams, by name.</summary>
        public List<TeamChoice> Teams { get; } = new List<TeamChoice>();

        /// <summary>The teams a bowl can take: the FBS teams not in a CFP game (those come from the poll).</summary>
        public List<TeamChoice> BowlTeams { get; } = new List<TeamChoice>();

        /// <summary>Why nothing can be edited (bowl week already under way, or no bowls yet); null when the games are editable.</summary>
        public string? NotEditableReason { get; }

        public BowlMatchups(FranchiseFile dynasty)
        {
            teams = dynasty.GetPrimaryTable("Team", "DisplayName");
            teams.ReadRecords();
            var bowls = dynasty.GetPrimaryTable("BowlGame");
            bowls.ReadRecords();
            seasonGames = dynasty.GetPrimaryTable("SeasonGame");
            seasonGames.ReadRecords();
            var seasonInfo = dynasty.GetPrimaryTable("SeasonInfo").Records.First(r => !r.IsEmpty);
            currentWeek = Convert.ToInt32(seasonInfo["CurrentWeek"]);
            requests = dynasty.GetPrimaryTable("SeasonGameRequest");
            requests.ReadRecords();

            foreach (var team in teams.Records.Where(t => !t.IsEmpty && (t["UserCharacter"] as string ?? NoRef) != NoRef))
            {
                UserTeams.Add(team.Index);
            }

            if (UserTeams.Count == 0)
            {
                var userCoach = dynasty.GetPrimaryTable("Coach").Records.FirstOrDefault(c => !c.IsEmpty && c["IsUserControlled"] is true);
                var userTeam = userCoach == null ? null : teams.Records.FirstOrDefault(t => !t.IsEmpty && Convert.ToInt32(t["TeamIndex"]) == Convert.ToInt32(userCoach["TeamIndex"]));
                if (userTeam != null)
                {
                    UserTeams.Add(userTeam.Index);
                }
            }

            var allTeams = teams.Records.Where(t => !t.IsEmpty && Convert.ToInt32(t["TeamIndex"]) != NoTeamIndex).ToArray();

            // only bowl eligble teams
            var eligbleTeams = allTeams
                .Where(t => (Convert.ToInt32(t["ConfWin"]) + Convert.ToInt32(t["NonConfWin"])) >= 6)
                .ToList();

            // if we dont have enough we need those with 5 wins
            if (eligbleTeams.Count < 76)
            {
                eligbleTeams.AddRange(allTeams.Where(t => (Convert.ToInt32(t["ConfWin"]) + Convert.ToInt32(t["NonConfWin"])) == 5));
            }

            foreach (var team in eligbleTeams)
            {
                int wins = Convert.ToInt32(team["ConfWin"]) + Convert.ToInt32(team["NonConfWin"]);
                int losses = Convert.ToInt32(team["ConfLoss"]) + Convert.ToInt32(team["NonConfLoss"]);
                int cfp = Convert.ToInt32(team["CFPPoll_CurrentRank"]);
                var name = team["DisplayName"] as string ?? string.Empty;
                Teams.Add(new TeamChoice(team.Index, name, $"{name} ({wins}-{losses}{(cfp >= 1 && cfp <= 25 ? $", CFP #{cfp}" : string.Empty)})"));
            }

            Teams.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            // earlier weeks' results are already posted
            var postseason = seasonGames.Records
                .Where(g => !g.IsEmpty && Convert.ToInt32(g["SeasonWeek"]) is int week && week >= Math.Max(FirstBowlWeek, currentWeek) && week <= LastEditableWeek)
                .ToList();
            if (currentWeek < FirstBowlWeek)
            {
                NotEditableReason = $"This save is from week {currentWeek}. The bowl matchups you see before bowl week are projections: " +
                    "advancing to bowl week picks every bowl again (and the CFP field from the CFP poll), undoing any change. " +
                    "Advance to bowl week 1 (week 17), save, and edit that save.";
            }
            else if (currentWeek > LastEditableWeek)
            {
                NotEditableReason = "The bowls and the CFP first round have already been played.";
            }

            // the seeds of each first-round game, for naming the quarterfinal it feeds
            var firstRoundSeeds = bowls.Records
                .Where(b => !b.IsEmpty && Convert.ToBoolean(b["IsPlayoffBowl"]) && Convert.ToInt32(b["PlayoffBracketSlot"]) < FirstQuarterfinalSlot)
                .GroupBy(b => Convert.ToInt32(b["PlayoffBracketSlot"]))
                .ToDictionary(g => g.Key, g => Seeds(g.First(), quarterfinal: false));

            foreach (var game in postseason)
            {
                var bowlRef = game.GetReferenceDataByKey("BowlGame");
                if (bowlRef?.TableId != bowls.Header.TableId)
                {
                    continue;
                }

                var bowl = bowls.Records[bowlRef.Value.RowNumber];
                bool playoff = Convert.ToBoolean(bowl["IsPlayoffBowl"]);
                int slot = playoff ? Convert.ToInt32(bowl["PlayoffBracketSlot"]) : -1;
                int? home = TeamRow(game, "HomeTeam");
                int? away = TeamRow(game, "AwayTeam");

                // a quarterfinal waits for its first-round winner; any other game without both teams isn't set yet
                bool quarterfinal = playoff && slot >= FirstQuarterfinalSlot;
                if (home == null || (away == null && !quarterfinal))
                {
                    continue;
                }

                int month = Convert.ToInt32(game["GameDateMonth"]);
                Games.Add(new BowlSlot(game)
                {
                    Week = Convert.ToInt32(game["SeasonWeek"]),
                    Date = $"{game["DayOfWeek"]?.ToString()?.Substring(0, 3)} {(month >= 1 && month <= 12 ? MonthNames[month] : "?")} {game["GameDateDay"]}",
                    SortKey = ((month < 7 ? month + 12 : month) * 100 + Convert.ToInt32(game["GameDateDay"])) * 10000 + Convert.ToInt32(game["TimeOfDay"]),
                    Bowl = BowlName(game, bowl, quarterfinal),
                    OrderName = OrderName(game, bowl, playoff, quarterfinal),
                    PlayoffSlot = slot,
                    Seeds = playoff ? Seeds(bowl, quarterfinal) : string.Empty,
                    WasSimulated = Played(game),
                    OriginalHome = home,
                    OriginalAway = away,
                    Home = home,
                    Away = away,
                    // the bracket comes from the CFP poll; shown, not edited
                    Locked = playoff,
                    // quarterfinal slot 4-7 gets the winner of first-round slot 0-3
                    AwayNote = quarterfinal ? $"Winner of {(firstRoundSeeds.TryGetValue(slot - FirstQuarterfinalSlot, out var fed) ? fed : "the first round")}" : null,
                });
            }

            var playoffTeams = Games.Where(g => g.IsPlayoff).SelectMany(g => new[] { g.Home, g.Away }).Where(r => r != null).ToHashSet();
            BowlTeams.AddRange(Teams.Where(t => !playoffTeams.Contains(t.Row)));

            var order = DisplayOrder.Select((name, i) => (Key: Normalize(name), Index: i)).ToDictionary(x => x.Key, x => x.Index);
            int Position(BowlSlot game) => order.TryGetValue(Normalize(game.OrderName), out var i) ? i : DisplayOrder.Length;
            Games.Sort((a, b) => Position(a) != Position(b) ? Position(a).CompareTo(Position(b))
                : a.Week != b.Week ? a.Week.CompareTo(b.Week)
                : a.SortKey.CompareTo(b.SortKey));
        }

        /// <summary>The grid's name for a game: a quarterfinal also names the bowl hosting it.</summary>
        private static string BowlName(FranchiseFileRecord game, FranchiseFileRecord bowl, bool quarterfinal)
        {
            var name = bowl["Name"] as string ?? string.Empty;
            return quarterfinal && HostBowl(game) is string host ? $"{host} ({name})" : name;
        }

        /// <summary>The game's name in <see cref="DisplayOrder"/>: "CFP 5 v 12" for a first-round game, the host for a quarterfinal.</summary>
        private static string OrderName(FranchiseFileRecord game, FranchiseFileRecord bowl, bool playoff, bool quarterfinal)
        {
            if (playoff && !quarterfinal)
            {
                return $"CFP {Convert.ToInt32(bowl["Conference1Rank"]) + 1} v {Convert.ToInt32(bowl["Conference2Rank"]) + 1}";
            }

            return quarterfinal ? HostBowl(game) ?? string.Empty : bowl["Name"] as string ?? string.Empty;
        }

        private static string? HostBowl(FranchiseFileRecord game)
        {
            var stadium = game["Stadium"] as string ?? NoRef;
            return stadium != NoRef && NewYearsSixBowls.TryGetValue(Convert.ToUInt32(stadium, 2), out var host) ? host : null;
        }

        private static string Normalize(string name) => new string(name.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

        public string TeamName(int? row) => row == null ? string.Empty : teams.Records[row.Value]["DisplayName"] as string ?? "?";

        /// <summary>Teams in more than one game, or on both sides of one.</summary>
        public List<string> Problems()
        {
            var problems = new List<string>();
            foreach (var game in Games.Where(g => g.Home != null && g.Home == g.Away))
            {
                problems.Add($"{game.Bowl}: {TeamName(game.Home)} can't play itself.");
            }

            var byTeam = Games
                .SelectMany(g => new[] { (Row: g.Home, Game: g), (Row: g.Away, Game: g) })
                .Where(x => x.Row != null)
                .GroupBy(x => x.Row!.Value)
                .Where(group => group.Select(x => x.Game).Distinct().Count() > 1);
            foreach (var group in byTeam)
            {
                problems.Add($"{TeamName(group.Key)} is in {string.Join(" and ", group.Select(x => x.Game.Bowl).Distinct())}.");
            }

            return problems;
        }

        /// <summary>Teams that had a game in these weeks and no longer do.</summary>
        public List<string> TeamsLeftOut()
        {
            var now = Games.SelectMany(g => new[] { g.Home, g.Away }).Where(r => r != null).ToHashSet();
            return Games.SelectMany(g => new[] { g.OriginalHome, g.OriginalAway })
                .Where(r => r != null && !now.Contains(r))
                .Select(r => TeamName(r))
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Puts a team on one side of a game. If the team was already in another editable game (or on the other side of
        /// this one), the team it replaces takes its old place, so the two swap; returns that other game, or null. A CFP
        /// team can't move, so picking one is left as a conflict.
        /// </summary>
        public BowlSlot? Assign(BowlSlot game, bool home, int? team)
        {
            var replaced = home ? game.Home : game.Away;
            BowlSlot? swapped = null;
            if (team != null && replaced != null && team != replaced)
            {
                foreach (var other in Games.Where(g => !g.Locked))
                {
                    if (other.Home == team && !(other == game && home))
                    {
                        other.Home = replaced;
                        swapped = other;
                        break;
                    }

                    if (other.Away == team && !(other == game && !home))
                    {
                        other.Away = replaced;
                        swapped = other;
                        break;
                    }
                }
            }

            if (home)
            {
                game.Home = team;
            }
            else
            {
                game.Away = team;
            }

            return swapped;
        }

        /// <summary>
        /// Writes the changed matchups into the save (in memory), putting an already simulated one back to Unplayed, and
        /// closes the user's play-your-game request if the user's team left this week's game; returns how many games changed.
        /// </summary>
        public int Apply()
        {
            int changed = 0;
            ClosedUserRequest = false;
            WrittenResults.Clear();
            var vacated = new List<BowlSlot>();
            foreach (var game in Games.Where(g => g.IsChanged && !g.Locked))
            {
                bool wasUserGame = UserTeams.Contains(game.OriginalHome ?? -1) || UserTeams.Contains(game.OriginalAway ?? -1);
                bool isUserGame = UserTeams.Contains(game.Home ?? -1) || UserTeams.Contains(game.Away ?? -1);
                if (game.Week == currentWeek && !game.WasSimulated && wasUserGame && !isUserGame)
                {
                    vacated.Add(game);
                }

                game.Record["HomeTeam"] = BinaryUtil.GetBinaryReferenceData(teams.Header.TableId, game.Home!.Value);
                game.Record["AwayTeam"] = BinaryUtil.GetBinaryReferenceData(teams.Header.TableId, game.Away!.Value);
                game.Record["IsRematch"] = MetBefore(game.Home.Value, game.Away.Value);
                if (game.WasSimulated)
                {
                    ResetToUnplayed(game.Record);
                    game.WasSimulated = false;
                }

                game.OriginalHome = game.Home;
                game.OriginalAway = game.Away;
                changed++;
            }

            if (vacated.Count > 0)
            {
                ClosedUserRequest = CloseUserRequests() > 0;
                foreach (var game in vacated)
                {
                    WriteFinalScore(game);
                }
            }

            return changed;
        }

        /// <summary>
        /// A final score for a game the game itself won't play (the user's vacated game), written as MyDynasty does: quarter
        /// scores, the winner as ForceWin, IsSimmed. The stronger team (TEAM_RATINGOVR) is more likely to win.
        /// </summary>
        private void WriteFinalScore(BowlSlot game)
        {
            var home = teams.Records[game.Home!.Value];
            var away = teams.Records[game.Away!.Value];
            int ratingGap = Convert.ToInt32(home["TEAM_RATINGOVR"]) - Convert.ToInt32(away["TEAM_RATINGOVR"]);
            // about 4 in 5 for a 10-point better team, 19 in 20 for 20 points
            bool homeWins = Random.Shared.NextDouble() < 1 / (1 + Math.Exp(-ratingGap / 7.0));

            int[] homeQuarters = Quarters(), awayQuarters = Quarters();
            int homeTotal = homeQuarters.Sum(), awayTotal = awayQuarters.Sum();
            if ((homeTotal > awayTotal) != homeWins || homeTotal == awayTotal)
            {
                // give the winner the better line; a tie gets a late field goal
                if (homeTotal != awayTotal)
                {
                    (homeQuarters, awayQuarters) = (awayQuarters, homeQuarters);
                }
                else
                {
                    (homeWins ? homeQuarters : awayQuarters)[3] += 3;
                }
            }

            var record = game.Record;
            for (int q = 0; q < 4; q++)
            {
                record[$"HomeScoreQuarter{q + 1}"] = homeQuarters[q];
                record[$"AwayScoreQuarter{q + 1}"] = awayQuarters[q];
            }

            record["HomeScore"] = homeQuarters.Sum();
            record["AwayScore"] = awayQuarters.Sum();
            record["GameStatus"] = homeWins ? "HomeWon" : "AwayWon";
            record["ForceWin"] = homeWins ? "Home" : "Away";
            record["IsSimmed"] = true;
            WrittenResults.Add($"{game.Bowl}: {away["DisplayName"]} {awayQuarters.Sum()}, {home["DisplayName"]} {homeQuarters.Sum()}");
        }

        /// <summary>One team's quarter-by-quarter points, from ordinary scoring drives.</summary>
        private static int[] Quarters()
        {
            int[] drives = { 0, 0, 0, 3, 3, 7, 7, 7, 10, 14 };
            return Enumerable.Range(0, 4).Select(_ => drives[Random.Shared.Next(drives.Length)]).ToArray();
        }

        /// <summary>
        /// Closes the user's open game requests the way the game closes an answered one: no TargetUser, the closed RequestId,
        /// ResolveTime = IssueTime, Resolution 1. Returns how many were closed.
        /// </summary>
        private int CloseUserRequests()
        {
            int closed = 0;
            foreach (var request in requests.Records.Where(r => !r.IsEmpty && (r["TargetUser"] as string ?? NoRef) != NoRef && Convert.ToInt32(r["Resolution"]) == 0))
            {
                request["TargetUser"] = NoRef;
                request["RequestId"] = Convert.ChangeType(ClosedRequestId, request["RequestId"]!.GetType());
                request["ResolveTime"] = request["IssueTime"];
                request["Resolution"] = Convert.ChangeType(1, request["Resolution"]!.GetType());
                closed++;
            }

            return closed;
        }

        /// <summary>
        /// Back to the state of a game the week hasn't played yet (as the user's own game is): no result, not simulated, and
        /// no links to the old result, which is then never posted.
        /// </summary>
        private static void ResetToUnplayed(FranchiseFileRecord game)
        {
            game["GameStatus"] = "Unplayed";
            game["IsSimmed"] = false;
            game["IsOvertimeGame"] = false;
            game["ForceWin"] = "None";
            foreach (var field in Scores)
            {
                game[field] = 0;
            }

            foreach (var field in ResultLinks)
            {
                game[field] = NoRef;
            }
        }

        /// <summary>Whether the two teams already played each other this season (SeasonGame.IsRematch).</summary>
        private bool MetBefore(int a, int b) => seasonGames.Records.Any(g => !g.IsEmpty && Convert.ToInt32(g["SeasonWeek"]) < FirstBowlWeek && Played(g)
            && TeamRow(g, "HomeTeam") is int home && TeamRow(g, "AwayTeam") is int away && ((home == a && away == b) || (home == b && away == a)));

        private static bool Played(FranchiseFileRecord game) =>
            game["GameStatus"] as string is "HomeWon" or "AwayWon";

        private int? TeamRow(FranchiseFileRecord game, string field)
        {
            var reference = game.GetReferenceDataByKey(field);
            return reference?.TableId == teams.Header.TableId ? reference.Value.RowNumber : null;
        }

        /// <summary>The seeds a playoff game is for (Conference1Rank / Conference2Rank hold them 0-based).</summary>
        private static string Seeds(FranchiseFileRecord bowl, bool quarterfinal)
        {
            int high = Convert.ToInt32(bowl["Conference1Rank"]) + 1;
            int low = Convert.ToInt32(bowl["Conference2Rank"]) + 1;
            return quarterfinal ? $"#{high} seed" : $"#{high} vs #{low}";
        }
    }

    internal sealed class BowlSlot
    {
        public BowlSlot(FranchiseFileRecord record)
        {
            Record = record;
        }

        public FranchiseFileRecord Record { get; }
        public int Week { get; init; }
        public string Date { get; init; } = string.Empty;
        public int SortKey { get; init; }
        public string Bowl { get; init; } = string.Empty;
        /// <summary>the name the grid is ordered by (see BowlMatchups.DisplayOrder)</summary>
        public string OrderName { get; init; } = string.Empty;
        /// <summary>PlayoffBracketSlot, -1 for a bowl</summary>
        public int PlayoffSlot { get; init; }
        public bool IsPlayoff => PlayoffSlot >= 0;
        public string Seeds { get; init; } = string.Empty;
        /// <summary>already simulated this week (results not yet posted); an edit puts it back to Unplayed</summary>
        public bool WasSimulated { get; set; }
        public int? OriginalHome { get; set; }
        public int? OriginalAway { get; set; }
        public int? Home { get; set; }
        public int? Away { get; set; }
        /// <summary>a CFP game: its teams come from the CFP poll (a quarterfinal's visitor is the first-round winner)</summary>
        public bool Locked { get; init; }
        public string? AwayNote { get; init; }
        public bool IsChanged => Home != OriginalHome || Away != OriginalAway;
    }

    internal sealed record TeamChoice(int Row, string Name, string Label);
}

using CFB27.Data.Model;
using MaddenFranchise;
using MaddenFranchise.Dynasty;
using NCAA14Utility;

namespace ScheduleMaker
{
    /// <summary>
    /// Builds the ScheduleModel (schedulemodel.txt) for a CFB 27 preseason schedule, the same summary EADBEditor's
    /// ScheduleFixup writes for NCAA 14: each FBS team's games, schedule notes, and league-wide game counts. Ids are the
    /// classic team and conference ids.
    /// </summary>
    internal sealed class ScheduleModelExporter
    {
        private const int NotreDameId = 68;
        private const int IndependentsId = 5;
        private const int AmericanId = 3;

        /// <summary>ACC, Big Ten, Big 12, Pac-12, SEC (Notre Dame also counts as P5 where noted).</summary>
        private static readonly HashSet<int> P5Conferences = new HashSet<int> { 0, 1, 2, 10, 11 };

        /// <summary>SeasonGame/known-game day -> the model's day (0 Monday ... 6 Sunday).</summary>
        private static readonly Dictionary<string, int> Days = new Dictionary<string, int>
        {
            { "Monday", 0 }, { "Tuesday", 1 }, { "Wednesday", 2 }, { "Thursday", 3 }, { "Friday", 4 }, { "Saturday", 5 }, { "Sunday", 6 },
        };

        private readonly PreseasonSchedule schedule;
        private readonly Dictionary<int, int> conferenceByTeamRow = new Dictionary<int, int>();
        private readonly Dictionary<int, int> conferenceGamesByConference = new Dictionary<int, int>();

        private ScheduleModelExporter(FranchiseFile file, PreseasonSchedule schedule)
        {
            this.schedule = schedule;

            var conferenceTable = file.GetPrimaryTable("Conference", "TeamSlots");
            foreach (var conference in CFBConference.Read(file))
            {
                conferenceGamesByConference[conference.ClassicId] = Convert.ToInt32(conferenceTable.Records[conference.Row]["NumConferenceGames"]);
                foreach (var row in conference.TeamRows)
                {
                    conferenceByTeamRow[row] = conference.ClassicId;
                }
            }
        }

        public static ScheduleModel Create(FranchiseFile file, PreseasonSchedule schedule) =>
            new ScheduleModelExporter(file, schedule).Build();

        #region teams

        private int TeamId(int row) => CFBTeam.TeamRowToOldTeamIdMap[row];

        /// <summary>The game's generic FCS opponents (FCS East, FCS West ...).</summary>
        private bool IsFcs(int row) => Convert.ToInt32(schedule.TeamTable.Records[row]["TeamIndex"]) == CFBPlayer.NoTeamIndex;

        private int Conference(int row) => conferenceByTeamRow.TryGetValue(row, out var conference) ? conference : -1;

        private bool IsP5(int row) => P5Conferences.Contains(Conference(row));

        private bool IsP5OrNotreDame(int row) => IsP5(row) || TeamId(row) == NotreDameId;

        /// <summary>MWC, MAC, Sun Belt, American, CUSA and the independents other than Notre Dame.</summary>
        private bool IsG5(int row) => !IsFcs(row) && !IsP5OrNotreDame(row);

        /// <summary>Short name as the old file had it; the generic FCS teams read "FCS E", "FCS MW" ...</summary>
        private string Abbreviation(int row)
        {
            var shortName = schedule.TeamTable.Records[row]["ShortName"] as string ?? string.Empty;
            return IsFcs(row) && shortName.StartsWith("FCS") && shortName.Length > 3 ? "FCS " + shortName.Substring(3) : shortName;
        }

        #endregion

        #region games

        private bool IsConferenceGame(ScheduledGame g) =>
            Conference(g.HomeTeamRow) >= 0 && Conference(g.HomeTeamRow) != IndependentsId && Conference(g.HomeTeamRow) == Conference(g.AwayTeamRow);

        private bool IsP5ConferenceGame(ScheduledGame g) => IsConferenceGame(g) && IsP5(g.HomeTeamRow) && IsP5(g.AwayTeamRow);

        private bool IsP5NonConferenceGame(ScheduledGame g) => !IsConferenceGame(g) && IsP5OrNotreDame(g.HomeTeamRow) && IsP5OrNotreDame(g.AwayTeamRow);

        /// <summary>A non-conference game with at least one P5 team (Notre Dame not counted).</summary>
        private bool IsP5GameAnyOpponent(ScheduledGame g) => !IsConferenceGame(g) && (IsP5(g.HomeTeamRow) || IsP5(g.AwayTeamRow));

        private bool IsG5Game(ScheduledGame g) =>
            !IsConferenceGame(g) && IsG5(g.HomeTeamRow) && IsG5(g.AwayTeamRow);

        private static int Opponent(ScheduledGame g, int teamRow) => g.HomeTeamRow == teamRow ? g.AwayTeamRow : g.HomeTeamRow;

        /// <summary>"vs gt" / "at fsu" for conference games, "VS ULM" / "AT MICH" otherwise, "G5-VS ..." for G5 games.</summary>
        private string OpponentText(ScheduledGame g, int teamRow)
        {
            var text = (g.HomeTeamRow == teamRow ? "vs " : "at ") + Abbreviation(Opponent(g, teamRow));
            if (IsG5Game(g))
            {
                text = "G5-" + text;
            }

            return IsConferenceGame(g) ? text.ToLowerInvariant() : text.ToUpperInvariant();
        }

        #endregion

        private ScheduleModel Build()
        {
            var model = new ScheduleModel();
            var games = schedule.Games.Where(g => g.IsEnabled && g.HomeTeamRow >= 0 && g.AwayTeamRow >= 0).ToList();
            int fcsGames = 0, g5Matchups = 0, p5Matchups = 0, p5VsG5 = 0;

            var teamRows = games.SelectMany(g => new[] { g.HomeTeamRow, g.AwayTeamRow })
                .Distinct()
                .Where(row => !IsFcs(row))
                .OrderBy(Conference)
                .ThenBy(row => schedule.TeamName(row));

            foreach (var row in teamRows)
            {
                var teamGames = games.Where(g => g.HomeTeamRow == row || g.AwayTeamRow == row).OrderBy(g => g.Week).ToList();
                var conference = Conference(row);
                var team = new TeamSeasonSchedule
                {
                    TeamId = TeamId(row),
                    TeamName = schedule.TeamName(row) ?? string.Empty,
                    TeamAbbreviation = Abbreviation(row),
                    ConferenceId = conference,
                };

                var homeGames = teamGames.Count(g => g.HomeTeamRow == row);
                var conferenceGames = teamGames.Where(IsConferenceGame).ToList();
                var ooc = teamGames.Where(g => !IsConferenceGame(g)).ToList();
                var p5Opponents = ooc.Count(IsP5NonConferenceGame);
                var fcsOpponents = ooc.Count(g => IsFcs(Opponent(g, row)));
                var g5Opponents = ooc.Count(IsG5Game);

                fcsGames += fcsOpponents;
                if (IsP5(row))
                {
                    p5Matchups += p5Opponents;
                    p5VsG5 += ooc.Count - p5Opponents - fcsOpponents;
                }

                if (IsG5(row))
                {
                    g5Matchups += g5Opponents;
                }

                AddNotes(team, row, conference, teamGames, conferenceGames, ooc, homeGames, p5Opponents, fcsOpponents);

                team.ScheduleNodes.AddRange(teamGames.Select(g => new ScheduleNode
                {
                    Week = g.Week + 1,
                    Day = Days.TryGetValue(g.DayOfWeek ?? string.Empty, out var day) ? day : Days["Saturday"],
                    OpponentId = TeamId(Opponent(g, row)),
                    Opponent = OpponentText(g, row),
                    IsConferenceGame = IsConferenceGame(g),
                    IsP5Game = IsP5NonConferenceGame(g) || IsP5ConferenceGame(g),
                    IsG5Game = IsG5Game(g),
                    IsHomeGame = g.HomeTeamRow == row,
                }));

                model.TeamSchedules.Add(team);
            }

            model.FCSGames = $"{fcsGames} FCS GAMES";
            model.G5Games = $"{g5Matchups / 2} G5 GAMES";
            model.P5Games = $"{p5Matchups / 2} P5 GAMES";
            model.P5G5Games = $"{p5VsG5} P5 vs G5 GAMES";
            model.TotalGames = $"{games.Count} total games";
            return model;
        }

        /// <summary>The same checks as ScheduleFixup; conference game counts come from each conference's NumConferenceGames.</summary>
        private void AddNotes(TeamSeasonSchedule team, int row, int conference, List<ScheduledGame> teamGames, List<ScheduledGame> conferenceGames,
            List<ScheduledGame> ooc, int homeGames, int p5Opponents, int fcsOpponents)
        {
            if (homeGames < 5 && IsP5OrNotreDame(row))
            {
                team.Notes.Add($"{homeGames} home games.");
            }

            if (teamGames.Count != 12)
            {
                team.Notes.Add("Wrong game count.");
            }

            if (IsP5(row) && p5Opponents != 1)
            {
                team.Notes.Add($"{p5Opponents} P5 Opponents.");
            }

            if (ooc.Count != ooc.Select(g => Opponent(g, row)).Distinct().Count() && ooc.Count(g => IsFcs(Opponent(g, row))) <= 1)
            {
                team.Notes.Add("Duplicate opponents.");
            }

            // small conferences may take two FCS games
            var conferenceSize = conferenceByTeamRow.Count(kvp => kvp.Value == conference && TeamId(kvp.Key) != NotreDameId);
            if (fcsOpponents > (conferenceSize <= 6 ? 2 : 1))
            {
                team.Notes.Add("Multiple FCS opponents.");
            }
            else if (fcsOpponents == 0 && IsP5(row))
            {
                team.Notes.Add("No FCS.");
            }

            if (fcsOpponents >= 1 && IsG5(row) && conference != AmericanId)
            {
                team.Notes.Add("G5-FCS MATCHUP.");
            }

            if (conference != IndependentsId && conferenceGamesByConference.TryGetValue(conference, out var expected))
            {
                if (conferenceGames.Count != expected)
                {
                    team.Notes.Add("Wrong conference game count.");
                }

                // half at home; an odd count can go either way
                var conferenceHomeGames = conferenceGames.Count(g => g.HomeTeamRow == row);
                if (conferenceHomeGames != expected / 2 && !(expected % 2 == 1 && conferenceHomeGames == expected / 2 + 1))
                {
                    team.Notes.Add("Wrong conference home game count.");
                }
            }

            const int G5Goal = 2;
            var p5OpponentsForG5 = ooc.Count(IsP5GameAnyOpponent);
            if (IsG5(row) && p5OpponentsForG5 > G5Goal)
            {
                team.G5Note = $"G5 with {p5OpponentsForG5} P5 Opponents.";
            }
            else if (IsG5(row) && p5OpponentsForG5 <= 1)
            {
                team.G5Note = $"G5 with not enough {p5OpponentsForG5} P5 Opponents.";
            }
        }
    }
}

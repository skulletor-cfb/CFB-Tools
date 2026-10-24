using CFB27.Data.Model;
using MaddenFranchise.Dynasty;

namespace ScheduleMaker
{
    /// <summary>
    /// Replaces a CFB 27 preseason schedule with an NCAA 14 one (the SCHD table dumped to CSV). Every NCAA 14 game is
    /// played at the home team's stadium. The CFB 27 teams that weren't in NCAA 14 play each other once (a round robin)
    /// and get an FCS home game as their 12th game.
    /// </summary>
    internal sealed class Ncaa14ScheduleImporter
    {
        /// <summary>NCAA 14 team ids whose CFB 27 team has a different classic id (34 is Appalachian State in NCAA 14).</summary>
        private static readonly Dictionary<int, int> OldIdAliases = new Dictionary<int, int> { 
            { 34, 901 },
            { 61, 903 },
            { 100, 904 },
            { 181, 902 },
            { 230, 905 },
        };

        /// <summary>GDAT: 0 Monday ... 6 Sunday.</summary>
        private static readonly string[] Days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        /// <summary>Weeks for the new teams: the FCS game first, then 11 round-robin weeks (byes in weeks 0, 8 and 14).</summary>
        private const int NewTeamFcsWeek = 1;
        private static readonly int[] NewTeamRoundRobinWeeks = { 2, 3, 4, 5, 6, 7, 9, 10, 11, 12, 13 };
        private const string NewTeamTimeSlot = "T1200";

        private const int GamesPerTeam = 12;

        private readonly PreseasonSchedule schedule;
        private readonly List<(string Name, int Minutes)> exactTimes;

        private Ncaa14ScheduleImporter(PreseasonSchedule schedule)
        {
            this.schedule = schedule;

            // exact kickoff slots ("T1530" = 3:30pm); ranges such as NIGHT_FIRST are skipped
            exactTimes = schedule.TimeSlotOptions
                .Where(t => t.Length == 5 && t[0] == 'T' && t.Skip(1).All(char.IsDigit))
                .Select(t => (t, int.Parse(t.Substring(1, 2)) * 60 + int.Parse(t.Substring(3, 2))))
                .ToList();
        }

        /// <summary>Rewrites the schedule from the CSV. Returns a summary followed by any problems found afterwards.</summary>
        public static List<string> Import(PreseasonSchedule schedule, string csvPath) =>
            new Ncaa14ScheduleImporter(schedule).Run(csvPath);

        private bool IsFcs(int row) => Convert.ToInt32(schedule.TeamTable.Records[row]["TeamIndex"]) == CFBPlayer.NoTeamIndex;

        private List<string> Run(string csvPath)
        {
            var imported = ReadCsv(csvPath);
            var importedTeams = imported.SelectMany(g => new[] { g.Home, g.Away }).ToHashSet();

            // FBS teams in the save that NCAA 14 didn't have, with the FCS team each one already plays (if any)
            var fcsOpponents = new Dictionary<int, int>();
            var newTeams = schedule.Games
                .SelectMany(g => new[] { g.HomeTeamRow, g.AwayTeamRow })
                .Distinct()
                .Where(row => row >= 0 && !IsFcs(row) && !importedTeams.Contains(row))
                .OrderBy(row => schedule.TeamName(row))
                .ToList();
            foreach (var game in schedule.Games)
            {
                if (newTeams.Contains(game.HomeTeamRow) && IsFcs(game.AwayTeamRow))
                {
                    fcsOpponents.TryAdd(game.HomeTeamRow, game.AwayTeamRow);
                }
            }

            var newGames = NewTeamGames(newTeams, fcsOpponents);
            var all = imported.Concat(newGames).ToList();

            // reuse the existing rows in order, then add rows for the rest
            var existing = schedule.Games.ToList();
            if (all.Count < existing.Count)
            {
                throw new InvalidOperationException($"The import has {all.Count} games but the save has {existing.Count}; games can't be removed.");
            }

            for (int i = 0; i < all.Count; i++)
            {
                var g = all[i];
                if (i < existing.Count)
                {
                    var game = existing[i];
                    game.SetHomeTeam(g.Home);
                    game.SetAwayTeam(g.Away);
                    game.Week = g.Week;
                    game.DayOfWeek = g.Day;
                    game.TimeSlot = g.TimeSlot;
                    game.IsKickoffGame = false;
                    game.StadiumId = 0;
                }
                else
                {
                    schedule.AddGame(g.Home, g.Away, g.Week, g.Day, g.TimeSlot);
                }
            }

            var report = new List<string>
            {
                $"Imported {imported.Count} NCAA 14 games and {newGames.Count} games for the {newTeams.Count} new teams " +
                $"({all.Count} games, {Math.Max(0, all.Count - existing.Count)} rows added).",
                "New teams (home-away): " + string.Join(", ", newTeams.Select(t =>
                    $"{schedule.TeamName(t)} {newGames.Count(g => g.Home == t)}-{newGames.Count(g => g.Away == t)}")),
            };
            report.AddRange(Check());
            return report;
        }

        private sealed record ImportedGame(int Home, int Away, int Week, string Day, string TimeSlot);

        private List<ImportedGame> ReadCsv(string csvPath)
        {
            var lines = File.ReadAllLines(csvPath).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            var header = lines[0].Split(',').Select(h => h.Trim()).ToList();
            int Column(string name) => header.IndexOf(name) is var i && i >= 0
                ? i
                : throw new InvalidDataException($"{Path.GetFileName(csvPath)} has no {name} column.");
            int away = Column("GATG"), home = Column("GHTG"), day = Column("GDAT"), time = Column("GTOD"), week = Column("SEWN");

            var map = CFBTeam.OldTeamIdToTeamRowMap;
            int TeamRow(int oldId) =>
                map.TryGetValue(OldIdAliases.TryGetValue(oldId, out var alias) ? alias : oldId, out var row)
                    ? row
                    : throw new InvalidDataException($"NCAA 14 team {oldId} has no CFB 27 team.");

            return lines.Skip(1)
                .Select(l => l.Split(','))
                .Select(c => new ImportedGame(
                    TeamRow(int.Parse(c[home])),
                    TeamRow(int.Parse(c[away])),
                    int.Parse(c[week]),
                    Days[int.Parse(c[day])],
                    ClosestTimeSlot(int.Parse(c[time]))))
                .ToList();
        }

        /// <summary>The exact kickoff slot nearest to a time in minutes after midnight (11:59pm becomes T2300).</summary>
        private string ClosestTimeSlot(int minutes) => exactTimes.OrderBy(t => Math.Abs(t.Minutes - minutes)).ThenBy(t => t.Minutes).First().Name;

        /// <summary>
        /// A round robin (circle method) over the round-robin weeks, plus an FCS home game in the FCS week. Each game goes to
        /// whichever team has fewer home games so far, so round-robin home games come out 5 or 6 per team (6-6 or 7-5 once
        /// the FCS home game is added).
        /// </summary>
        private List<ImportedGame> NewTeamGames(List<int> teams, Dictionary<int, int> fcsOpponents)
        {
            var games = new List<ImportedGame>();
            if (teams.Count == 0)
            {
                return games;
            }

            var homeCount = teams.ToDictionary(t => t, _ => 0);
            var slots = teams.Cast<int?>().ToList();
            if (slots.Count % 2 == 1)
            {
                slots.Add(null); // bye
            }

            int rounds = slots.Count - 1;
            if (rounds > NewTeamRoundRobinWeeks.Length)
            {
                throw new InvalidOperationException($"{teams.Count} new teams need {rounds} round-robin weeks; only {NewTeamRoundRobinWeeks.Length} are set aside.");
            }

            for (int round = 0; round < rounds; round++)
            {
                for (int i = 0; i < slots.Count / 2; i++)
                {
                    var a = slots[i];
                    var b = slots[slots.Count - 1 - i];
                    if (a == null || b == null)
                    {
                        continue;
                    }

                    // fewer home games hosts; on a tie alternate by round so nobody hosts in long runs
                    bool aHosts = homeCount[a.Value] != homeCount[b.Value] ? homeCount[a.Value] < homeCount[b.Value] : (round + i) % 2 == 0;
                    int host = aHosts ? a.Value : b.Value, visitor = aHosts ? b.Value : a.Value;
                    homeCount[host]++;
                    games.Add(new ImportedGame(host, visitor, NewTeamRoundRobinWeeks[round], "Saturday", NewTeamTimeSlot));
                }

                // rotate everyone but the first slot
                var last = slots[^1];
                slots.RemoveAt(slots.Count - 1);
                slots.Insert(1, last);
            }

            // FCS opponents never host in CFB 27, so the 12th game is at home; teams without one get a generic FCS team in turn
            var genericFcs = schedule.TeamTable.Records.Where(r => !r.IsEmpty && IsFcs(r.Index) && !string.IsNullOrEmpty(r["DisplayName"] as string))
                .Select(r => r.Index).ToList();
            int next = 0;
            foreach (var team in teams)
            {
                int fcs = fcsOpponents.TryGetValue(team, out var opponent) ? opponent : genericFcs[next++ % genericFcs.Count];
                games.Add(new ImportedGame(team, fcs, NewTeamFcsWeek, "Saturday", NewTeamTimeSlot));
            }

            return games;
        }

        /// <summary>FBS teams without 12 games, or playing twice in a week.</summary>
        private IEnumerable<string> Check()
        {
            var bookings = schedule.Games
                .SelectMany(g => new[] { (Team: g.HomeTeamRow, g.Week), (Team: g.AwayTeamRow, g.Week) })
                .Where(b => b.Team >= 0 && !IsFcs(b.Team))
                .ToList();

            foreach (var team in bookings.GroupBy(b => b.Team).Where(t => t.Count() != GamesPerTeam))
            {
                yield return $"{schedule.TeamName(team.Key)} has {team.Count()} games.";
            }

            foreach (var week in bookings.GroupBy(b => b).Where(w => w.Count() > 1))
            {
                yield return $"{schedule.TeamName(week.Key.Team)} plays {week.Count()} games in week {week.Key.Week}.";
            }
        }
    }
}

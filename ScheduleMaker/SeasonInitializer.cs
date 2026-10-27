using MaddenFranchise;
using MaddenFranchise.Dynasty;
using NCAA14Utility.Scheduling;
using System.Text.RegularExpressions;

namespace ScheduleMaker
{
    /// <summary>
    /// Ctrl+Z. Initializes the season the game generated (EADBEditor's Big12Schedule.ProcessSchedule, adapted):
    /// 1. tops the FCS games up to <see cref="ExpectedFcsGames"/> with FCS-vs-FCS placeholders, which are swapped into
    ///    FBS non-conference games once the independents' and conferences' games are set; nothing breaks up a rivalry
    ///    (the save's Rivalry table) except a conference's own plan;
    /// 2. P5 teams host their non-P5 non-conference opponents (Notre Dame's games and neutral sites are left alone);
    /// 3. each conference's games become its ScheduleBase plan for the year: pairings in the plan keep their game and
    ///    get the plan's host (even years a team hosts the teams in its list, odd years it visits them), the rest are
    ///    reused for the plan's missing pairings;
    /// 4. locked games go to their week (ScheduleBase.IsLocked: Army-Navy and Notre Dame's road rivalry in week 13, its
    ///    home rivalry in week 7, the other rivalries where the game put them) and Notre Dame's rivalry hosts are set;
    /// 5. every other game is arranged in weeks 0-13 so that no team plays twice in a week, goes more than
    ///    <see cref="MaxGamesWithoutBye"/> games without a bye, more than <see cref="MaxHomeInARow"/> home games (four to
    ///    open the season) or
    ///    <see cref="MaxRoadInARow"/> road games in a row, everyone plays in weeks 0 and 13, and non-conference games (other than FCS games, which can
    ///    go late) are played by week <see cref="LastNonConferenceWeek"/>.
    /// </summary>
    internal static class SeasonInitializer
    {
        /// <summary>FCS games a season should start with.</summary>
        public const int ExpectedFcsGames = 110;

        private const int FirstWeek = 0;
        // nothing is played in week 14
        private const int LastScheduledWeek = 13;
        private const int LastNonConferenceWeek = 4;
        private const int MaxGamesWithoutBye = 6;
        private const int MaxHomeInARow = 3;
        // a season can open with one more
        private const int MaxOpeningHomeInARow = 4;
        private const int MaxRoadInARow = 2;

        private static readonly HashSet<int> FcsTeams = new HashSet<int>
        {
            ScheduleBase.FCSEast, ScheduleBase.FCSMidwest, ScheduleBase.FCSNorthwest, ScheduleBase.FCSSoutheast, ScheduleBase.FCSWest,
        };

        private static readonly HashSet<int> P5Conferences = new HashSet<int>
        {
            ScheduleHelpers.ACC, ScheduleHelpers.Big10, ScheduleHelpers.Big12, ScheduleHelpers.Pac12, ScheduleHelpers.SEC,
        };

        /// <summary>The season's year from a preseason save's name: DYNASTY-Y588P is 2588 (a "-EDIT" style suffix is fine).</summary>
        public static int YearFromFileName(string path)
        {
            var match = Regex.Match(Path.GetFileName(path), @"^DYNASTY-Y(\d{3,4})P", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                throw new InvalidOperationException($"{Path.GetFileName(path)} isn't named like a preseason save (DYNASTY-YxxxP, e.g. DYNASTY-Y588P for 2588).");
            }

            int year = int.Parse(match.Groups[1].Value);
            return year < 1000 ? 2000 + year : year;
        }

        /// <summary>FBS games against a generic FCS team (an FCS-vs-FCS placeholder counts once for each FCS team).</summary>
        public static int CountFcsGames(PreseasonSchedule schedule) =>
            schedule.Games.Sum(g => (FcsTeams.Contains(g.HomeTeamRow) ? 1 : 0) + (FcsTeams.Contains(g.AwayTeamRow) ? 1 : 0));

        /// <summary>
        /// Tops the season up to <see cref="ExpectedFcsGames"/> with FCS-vs-FCS placeholder games, one per week from week 1,
        /// cycling through the generic FCS teams. Each placeholder holds two FCS opponents to swap into FBS games.
        /// Returns how many were added.
        /// </summary>
        public static int AddFcsPlaceholders(PreseasonSchedule schedule)
        {
            int missing = ExpectedFcsGames - CountFcsGames(schedule);
            var fcs = FcsTeams.OrderBy(t => t).ToArray();
            int added = 0;
            for (int slot = 0; missing > 0; slot += 2, missing -= 2, added++)
            {
                int week = 1 + added % LastScheduledWeek;
                schedule.AddGame(fcs[slot % fcs.Length], fcs[(slot + 1) % fcs.Length], week);
            }

            return added;
        }

        /// <summary>
        /// Runs all the steps for <paramref name="year"/>. The FCS placeholders become the baseline Validate compares
        /// against (as an import does). Returns a report: a line per step and conference, then what still breaks a rule.
        /// </summary>
        public static List<string> Initialize(FranchiseFile dynasty, PreseasonSchedule schedule, int year)
        {
            var report = new List<string>();
            bool evenYear = year % 2 == 0;
            report.Add($"{year} is an {(evenYear ? "even" : "odd")} year: teams {(evenYear ? "host" : "visit")} the teams in their plan's list.");

            int fcsGames = CountFcsGames(schedule);
            int placeholders = AddFcsPlaceholders(schedule);
            if (placeholders > 0)
            {
                schedule.AcceptChanges();
            }

            report.Add(placeholders == 0
                ? $"{fcsGames} FCS games."
                : $"{fcsGames} FCS games; added {placeholders} FCS-vs-FCS games to swap into FBS games, for {CountFcsGames(schedule)}.");

            var rivalries = ReadRivalries(dynasty, schedule);
            report.AddRange(SetRequiredGames(schedule, RequiredGames.For(year), rivalries, evenYear));

            var plans = new List<ScheduleBase>();
            foreach (var type in typeof(ScheduleBase).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ScheduleBase)) && !t.IsAbstract)
                .OrderBy(t => t.Name))
            {
                var plan = (ScheduleBase)Activator.CreateInstance(type, year)!;
                try
                {
                    plan.Init();
                    plans.Add(plan);
                    report.AddRange(SetPlan(schedule, plan, type.Name));
                }
                catch (Exception ex)
                {
                    report.Add($"{type.Name}: not applied, its plan doesn't fit the save ({ex.Message}).");
                }
            }

            report.AddRange(PairIndependents(schedule, rivalries, evenYear));
            report.AddRange(SwapInFcsPlaceholders(schedule, rivalries, evenYear));
            report.Add(HostAtP5(schedule));
            var lockNotes = new List<string>();
            var locked = ApplyLocks(schedule, plans, evenYear, lockNotes);
            report.Add($"{locked.Count} locked games in their weeks: {string.Join(", ", locked.OrderBy(g => g.Week).Select(g => $"{Describe(schedule, g)} (week {g.Week})"))}.");
            report.AddRange(lockNotes);

            var moved = new Arrangement(schedule, locked).Solve(new Random(year));
            report.Add($"{moved} games moved to another week.");
            report.AddRange(Summary(schedule));
            return report;
        }

        /// <summary>The games <see cref="RequiredGames"/> set up; nothing later trades them away.</summary>
        private static HashSet<ScheduledGame> requiredGames = new HashSet<ScheduledGame>();

        /// <summary>
        /// Makes every required game exist with its host: a game the two teams already play gets the host, a missing one
        /// is made by a trade (A vs X and B vs Y become A vs B and X vs Y, so every team keeps its number of games),
        /// with X and Y in different conferences (at most one of them an FCS team) and not already meeting, and neither
        /// game required, locked or Notre Dame's.
        /// </summary>
        private static IEnumerable<string> SetRequiredGames(PreseasonSchedule schedule, IReadOnlyList<(int Away, int Home)> required, HashSet<(int, int)> rivalries, bool evenYear)
        {
            requiredGames = new HashSet<ScheduledGame>();
            if (required.Count == 0)
            {
                yield break;
            }

            var requiredPairs = new HashSet<(int, int)>(required.Select(r => Pair(r.Away, r.Home)));
            int Conference(int team) => ScheduleHelpers.TeamAndConferences.TryGetValue(team, out var c) ? c : -1;
            ScheduledGame? Between(int a, int b) => schedule.Games.FirstOrDefault(g => Pair(g.HomeTeamRow, g.AwayTeamRow) == Pair(a, b));
            int Opponent(ScheduledGame g, int team) => g.HomeTeamRow == team ? g.AwayTeamRow : g.HomeTeamRow;
            List<ScheduledGame> Tradeable(int team) => schedule.GamesFor(team)
                .Where(g => !requiredPairs.Contains(Pair(g.HomeTeamRow, g.AwayTeamRow)) && !rivalries.Contains(Pair(g.HomeTeamRow, g.AwayTeamRow)) &&
                    Opponent(g, team) != ScheduleBase.NotreDame &&
                    !ScheduleBase.IsSharedLock(g.HomeTeamRow, g.AwayTeamRow, evenYear, g.Week, out _))
                .ToList();

            int existed = 0, flipped = 0, traded = 0;
            var missing = new List<string>();
            foreach (var (awayTeam, homeTeam) in required)
            {
                var game = Between(awayTeam, homeTeam);
                if (game != null)
                {
                    existed++;
                    if (game.HomeTeamRow != homeTeam)
                    {
                        game.FlipHomeAndAway();
                        flipped++;
                    }

                    requiredGames.Add(game);
                    continue;
                }

                // prefer handing an FCS opponent on, then opponents of the same tier; never two FCS opponents, which would
                // turn two FBS games against FCS teams into an FCS-vs-FCS game
                var swap = Tradeable(homeTeam)
                    .SelectMany(ga => Tradeable(awayTeam).Select(gb => (Ga: ga, Gb: gb, X: Opponent(ga, homeTeam), Y: Opponent(gb, awayTeam))))
                    .Where(s => s.X != s.Y && !(FcsTeams.Contains(s.X) && FcsTeams.Contains(s.Y)) && Conference(s.X) != Conference(s.Y) && Between(s.X, s.Y) == null)
                    .OrderBy(s => FcsTeams.Contains(s.X) || FcsTeams.Contains(s.Y) ? 0 : 1)
                    .ThenBy(s => IsP5(s.X) == IsP5(s.Y) ? 0 : 1)
                    .ThenBy(s => s.Ga.Week).ThenBy(s => s.Gb.Week)
                    .FirstOrDefault();
                if (swap.Ga == null)
                {
                    missing.Add($"{schedule.TeamName(awayTeam)} at {schedule.TeamName(homeTeam)}");
                    continue;
                }

                swap.Ga.SetHomeTeam(homeTeam);
                swap.Ga.SetAwayTeam(awayTeam);
                swap.Gb.SetHomeTeam(swap.X);
                swap.Gb.SetAwayTeam(swap.Y);
                foreach (var g in new[] { swap.Ga, swap.Gb })
                {
                    // a neutral site or kickoff billing belonged to the old pairing
                    g.StadiumId = 0;
                    g.IsKickoffGame = false;
                }

                requiredGames.Add(swap.Ga);
                traded++;
            }

            yield return $"{required.Count} required games: {existed} already scheduled ({flipped} with home and away flipped), {traded} made by trading games.";
            if (missing.Count > 0)
            {
                yield return $"Required games that couldn't be made: {string.Join(", ", missing)}.";
            }
        }

        /// <summary>
        /// The independents other than Notre Dame, when there are more than Notre Dame and UConn. They play one another
        /// late in the season like a conference, so games between two of them count as conference games.
        /// </summary>
        private static HashSet<int> independentConference = new HashSet<int>();

        /// <summary>
        /// When there are more independents than Notre Dame and UConn, the others play one another late in the season:
        /// an independent with more non-FCS games against outsiders (conference teams, Notre Dame) than fit in weeks
        /// 0-<see cref="LastNonConferenceWeek"/> trades them, two at a time with another independent it doesn't play yet
        /// (one with too many outsiders itself first), for a game against it. A vs X and B vs Y become A vs B and X vs Y,
        /// so every team keeps its number of games; X and Y must be in different conferences and not already meet.
        /// Locked games and Notre Dame's games aren't traded.
        /// </summary>
        private static IEnumerable<string> PairIndependents(PreseasonSchedule schedule, HashSet<(int, int)> rivalries, bool evenYear)
        {
            var independents = ScheduleHelpers.TeamAndConferences.Where(kvp => kvp.Value == ScheduleHelpers.Independent).Select(kvp => kvp.Key).ToList();
            independentConference = independents.Count > 2
                ? new HashSet<int>(independents.Where(t => t != ScheduleBase.NotreDame))
                : new HashSet<int>();
            if (independentConference.Count == 0)
            {
                yield break;
            }

            const int EarlySlots = LastNonConferenceWeek - FirstWeek + 1;
            bool Outsider(int team) => !independentConference.Contains(team) && !FcsTeams.Contains(team);
            int Conference(int team) => ScheduleHelpers.TeamAndConferences.TryGetValue(team, out var c) ? c : -1;
            bool Meet(int a, int b) => schedule.Games.Any(g => (g.HomeTeamRow == a && g.AwayTeamRow == b) || (g.HomeTeamRow == b && g.AwayTeamRow == a));
            int Opponent(ScheduledGame g, int team) => g.HomeTeamRow == team ? g.AwayTeamRow : g.HomeTeamRow;
            // games an independent can trade: against an outsider other than Notre Dame, not locked
            List<ScheduledGame> Tradeable(int team) => schedule.GamesFor(team)
                .Where(g => Outsider(Opponent(g, team)) && Opponent(g, team) != ScheduleBase.NotreDame && !requiredGames.Contains(g) &&
                    !rivalries.Contains(Pair(g.HomeTeamRow, g.AwayTeamRow)) &&
                    !ScheduleBase.IsSharedLock(g.HomeTeamRow, g.AwayTeamRow, evenYear, g.Week, out _))
                .ToList();
            int Excess(int team) => schedule.GamesFor(team).Count(g => Outsider(Opponent(g, team))) - EarlySlots;
            int HomeGames(int team) => schedule.GamesFor(team).Count(g => g.HomeTeamRow == team);

            int trades = 0;
            bool traded = true;
            while (traded)
            {
                traded = false;
                var needy = independentConference.Where(t => Excess(t) > 0).OrderByDescending(Excess).ThenBy(t => t).ToList();
                foreach (var a in needy)
                {
                    foreach (var b in independentConference.Where(b => b != a && !Meet(a, b)).OrderByDescending(Excess).ThenBy(b => b).ToList())
                    {
                        // X and Y: in different conferences, not already meeting; prefer opponents of the same tier
                        var swap = Tradeable(a)
                            .SelectMany(ga => Tradeable(b).Select(gb => (Ga: ga, Gb: gb, X: Opponent(ga, a), Y: Opponent(gb, b))))
                            .Where(s => s.X != s.Y && Conference(s.X) != Conference(s.Y) && !Meet(s.X, s.Y))
                            .OrderBy(s => IsP5(s.X) == IsP5(s.Y) ? 0 : 1)
                            .ThenBy(s => s.Ga.Week).ThenBy(s => s.Gb.Week)
                            .FirstOrDefault();
                        if (swap.Ga == null || Excess(a) <= 0)
                        {
                            continue;
                        }

                        int host = HomeGames(a) <= HomeGames(b) ? a : b;
                        swap.Ga.SetHomeTeam(host);
                        swap.Ga.SetAwayTeam(host == a ? b : a);
                        swap.Gb.SetHomeTeam(swap.X);
                        swap.Gb.SetAwayTeam(swap.Y);
                        foreach (var game in new[] { swap.Ga, swap.Gb })
                        {
                            // a neutral site or kickoff billing belonged to the old pairing
                            game.StadiumId = 0;
                            game.IsKickoffGame = false;
                        }

                        trades++;
                        traded = true;
                    }
                }
            }

            var left = independentConference.Where(t => Excess(t) > 0).OrderBy(t => schedule.TeamName(t)).ToList();
            yield return $"{independentConference.Count} independents play one another late: {trades} trades made" +
                (left.Count > 0 ? $"; still more than {EarlySlots} games against outsiders: {string.Join(", ", left.Select(t => $"{schedule.TeamName(t)} ({Excess(t) + EarlySlots})"))}." : ".");
        }

        /// <summary>
        /// Swaps each FCS-vs-FCS placeholder into an FBS non-conference game: placeholder F1 vs F2 and game A vs B become
        /// A vs F1 and B vs F2, the FBS teams at home, so every team keeps its number of games. The game broken up is
        /// never a rivalry (the save's Rivalry table), locked, required, Notre Dame's, at a neutral site or a kickoff game;
        /// of the rest, the one whose teams have the fewest FCS games (so a team gets a second only when it must), then
        /// not P5 against P5, then the latest.
        /// </summary>
        private static IEnumerable<string> SwapInFcsPlaceholders(PreseasonSchedule schedule, HashSet<(int, int)> rivalries, bool evenYear)
        {
            var placeholders = schedule.Games.Where(g => FcsTeams.Contains(g.HomeTeamRow) && FcsTeams.Contains(g.AwayTeamRow)).ToList();
            if (placeholders.Count == 0)
            {
                yield break;
            }

            int FcsGames(int team) => schedule.GamesFor(team).Count(g => FcsTeams.Contains(g.HomeTeamRow) || FcsTeams.Contains(g.AwayTeamRow));
            var candidates = schedule.Games.Where(g =>
                    !FcsTeams.Contains(g.HomeTeamRow) && !FcsTeams.Contains(g.AwayTeamRow) && !IsConferenceGame(g) &&
                    !rivalries.Contains(Pair(g.HomeTeamRow, g.AwayTeamRow)) && !requiredGames.Contains(g) &&
                    g.HomeTeamRow != ScheduleBase.NotreDame && g.AwayTeamRow != ScheduleBase.NotreDame &&
                    g.StadiumId == 0 && !g.IsKickoffGame &&
                    !ScheduleBase.IsSharedLock(g.HomeTeamRow, g.AwayTeamRow, evenYear, g.Week, out _))
                .ToList();

            int swapped = 0;
            var doubled = new List<string>();
            foreach (var placeholder in placeholders)
            {
                var game = candidates
                    .OrderBy(g => Math.Max(FcsGames(g.HomeTeamRow), FcsGames(g.AwayTeamRow)))
                    .ThenBy(g => FcsGames(g.HomeTeamRow) + FcsGames(g.AwayTeamRow))
                    .ThenBy(g => IsP5(g.HomeTeamRow) && IsP5(g.AwayTeamRow) ? 1 : 0)
                    .ThenByDescending(g => g.Week)
                    .FirstOrDefault();
                if (game == null)
                {
                    yield return $"No non-conference game left to swap {Describe(schedule, placeholder)} into.";
                    break;
                }

                candidates.Remove(game);
                int a = game.HomeTeamRow, b = game.AwayTeamRow, f1 = placeholder.HomeTeamRow, f2 = placeholder.AwayTeamRow;
                placeholder.SetHomeTeam(a);
                placeholder.SetAwayTeam(f1);
                placeholder.Week = game.Week;
                game.SetHomeTeam(b);
                game.SetAwayTeam(f2);
                swapped++;

                foreach (var team in new[] { a, b }.Where(t => FcsGames(t) > 1))
                {
                    doubled.Add(schedule.TeamName(team) ?? team.ToString());
                }
            }

            yield return $"{swapped} FCS-vs-FCS games swapped into FBS games, for {schedule.Games.Count(g => FcsTeams.Contains(g.HomeTeamRow) != FcsTeams.Contains(g.AwayTeamRow))} FBS games against FCS teams" +
                (doubled.Count > 0 ? $"; {doubled.Count} teams got a second FCS game ({string.Join(", ", doubled)})." : ".");
        }

        /// <summary>The save's rivalries as team-row pairs (Rivalry.Team1 / Team2).</summary>
        private static HashSet<(int, int)> ReadRivalries(FranchiseFile dynasty, PreseasonSchedule schedule)
        {
            var rivalries = new HashSet<(int, int)>();
            var table = dynasty.GetPrimaryTable("Rivalry");
            if (table == null)
            {
                return rivalries;
            }

            table.ReadRecords();
            int teamTableId = schedule.TeamTable.Header.TableId;
            foreach (var rivalry in table.Records.Where(r => !r.IsEmpty))
            {
                var team1 = rivalry.GetReferenceDataByKey("Team1");
                var team2 = rivalry.GetReferenceDataByKey("Team2");
                if (team1?.TableId == teamTableId && team2?.TableId == teamTableId)
                {
                    rivalries.Add(Pair(team1.Value.RowNumber, team2.Value.RowNumber));
                }
            }

            return rivalries;
        }

        /// <summary>
        /// FBS teams host FCS teams, and P5 teams host their other non-P5 non-conference opponents (Notre Dame's games
        /// aside). Neutral sites are left alone.
        /// </summary>
        private static string HostAtP5(PreseasonSchedule schedule)
        {
            int flipped = 0;
            foreach (var game in schedule.Games.Where(g => g.StadiumId == 0 &&
                ((FcsTeams.Contains(g.HomeTeamRow) && !FcsTeams.Contains(g.AwayTeamRow)) ||
                 (g.HomeTeamRow != ScheduleBase.NotreDame && g.AwayTeamRow != ScheduleBase.NotreDame && IsP5(g.AwayTeamRow) && !IsP5(g.HomeTeamRow)))))
            {
                game.FlipHomeAndAway();
                flipped++;
            }

            return $"FBS teams host FCS teams and P5 teams host non-P5 teams: {flipped} games flipped.";
        }

        /// <summary>
        /// The conference's games become its plan: pairings in the plan keep their game and get the plan's host, the rest
        /// are reused for the plan's missing pairings. Weeks are arranged later.
        /// </summary>
        private static IEnumerable<string> SetPlan(PreseasonSchedule schedule, ScheduleBase plan, string name)
        {
            var opponents = plan.ConferenceSchedule;
            var hosts = plan.ScenarioForSeason;

            // even years: a team hosts the teams in its list; odd years: they host it
            int HomeTeam(int a, int b) => (hosts.TryGetValue(a, out var list) && list.Contains(b)) == plan.IsEvenYear ? a : b;

            var needed = new HashSet<(int, int)>(opponents.SelectMany(kvp => kvp.Value.Select(opp => Pair(kvp.Key, opp))));
            var kept = new List<ScheduledGame>();
            var freed = new List<ScheduledGame>();
            foreach (var game in schedule.Games.Where(g => opponents.ContainsKey(g.HomeTeamRow) && opponents.ContainsKey(g.AwayTeamRow)))
            {
                (needed.Remove(Pair(game.HomeTeamRow, game.AwayTeamRow)) ? kept : freed).Add(game);
            }

            int flipped = 0;
            foreach (var game in kept.Where(g => HomeTeam(g.HomeTeamRow, g.AwayTeamRow) != g.HomeTeamRow))
            {
                game.FlipHomeAndAway();
                flipped++;
            }

            var newPairs = needed.OrderBy(p => p).Take(freed.Count).ToList();
            for (int i = 0; i < newPairs.Count; i++)
            {
                var game = freed[i];
                int home = HomeTeam(newPairs[i].Item1, newPairs[i].Item2);
                game.SetHomeTeam(home);
                game.SetAwayTeam(home == newPairs[i].Item1 ? newPairs[i].Item2 : newPairs[i].Item1);
                // a neutral site or kickoff billing belonged to the old pairing
                game.StadiumId = 0;
                game.IsKickoffGame = false;
            }

            yield return $"{name}: {kept.Count} games kept ({flipped} with home and away flipped), {newPairs.Count} changed to the plan's pairings.";
            if (freed.Count != needed.Count)
            {
                yield return $"{name}: the game has {freed.Count} conference games outside the plan but the plan is missing {needed.Count}; " +
                    $"{Math.Abs(freed.Count - needed.Count)} {(freed.Count > needed.Count ? "extra games were left as they were" : "pairings couldn't be placed")}.";
            }
        }

        /// <summary>
        /// Puts the locked games in their weeks and sets Notre Dame's rivalry hosts. A game asks the shared locks, then its
        /// own conference's, then every other conference's (their lists include rivalries with outsiders, Iowa-Iowa State
        /// say). When two locked games put a team in the same week, the one locked to a week beats one that only mustn't
        /// move, then the first found wins; the other is left for the arrangement and reported. Returns the locked games.
        /// </summary>
        private static HashSet<ScheduledGame> ApplyLocks(PreseasonSchedule schedule, List<ScheduleBase> plans, bool evenYear, List<string> report)
        {
            var found = new List<(ScheduledGame Game, int Week, bool Stays)>();
            foreach (var game in schedule.Games)
            {
                int host = ScheduleBase.NotreDameRivalryHost(game.HomeTeamRow, game.AwayTeamRow, evenYear);
                if (host >= 0 && game.HomeTeamRow != host)
                {
                    game.FlipHomeAndAway();
                }

                int week = -1;
                bool isLocked = ScheduleBase.IsSharedLock(game.HomeTeamRow, game.AwayTeamRow, evenYear, game.Week, out week) ||
                    plans.OrderBy(p => p.ConferenceSchedule.ContainsKey(game.HomeTeamRow) && p.ConferenceSchedule.ContainsKey(game.AwayTeamRow) ? 0 : 1)
                        .Any(p => p.IsLocked(game.HomeTeamRow, game.AwayTeamRow, game.Week, out week));
                if (isLocked)
                {
                    found.Add((game, week, week == game.Week));
                }
            }

            var locked = new HashSet<ScheduledGame>();
            var booked = new Dictionary<(int Team, int Week), ScheduledGame>();
            foreach (var (game, week, _) in found.OrderBy(f => f.Stays ? 1 : 0))
            {
                var teams = new[] { game.HomeTeamRow, game.AwayTeamRow }.Where(t => !FcsTeams.Contains(t)).ToList();
                var clash = teams.Select(t => booked.TryGetValue((t, week), out var other) ? other : null).FirstOrDefault(o => o != null);
                if (clash != null)
                {
                    report.Add($"{Describe(schedule, game)} is locked to week {week}, but {Describe(schedule, clash)} is too; it's left for the arrangement.");
                    continue;
                }

                game.Week = week;
                locked.Add(game);
                teams.ForEach(t => booked[(t, week)] = game);
            }

            return locked;
        }

        /// <summary>
        /// Every game with an FBS team in it, arranged in weeks 0-13 around the locked games. A local search (simulated
        /// annealing, seeded by the year so the same save always comes out the same) minimizes each team's
        /// <see cref="SeasonCost"/> plus <see cref="LateNonConferenceCost"/> for each week a non-conference game (other
        /// than an FCS game) is played after week <see cref="LastNonConferenceWeek"/>, <see cref="ConferenceWeekZeroCost"/>
        /// for a conference game in week 0 (week 0 is for non-conference and FCS games) and <see cref="EarlyConferenceCost"/>
        /// for each week a conference game is played before week 13, <see cref="EarlyConferenceZoneCost"/> for a conference
        /// game before week 4, <see cref="LateFcsCost"/> for each week an FCS game is played after week 4,
        /// <see cref="LastWeekFcsCost"/> for an FCS game in week 13 and <see cref="ShortWeekCost"/> for
        /// each game a week is short of its share (so byes don't pile up in one week), starting from <see cref="Seed"/>.
        /// </summary>
        private sealed class Arrangement
        {
            private const int Iterations = 16_000_000;
            private const double StartTemperature = 30, EndTemperature = 0.05;

            private readonly List<ScheduledGame> games;
            private readonly int[] weeks;
            private readonly int[] originalWeeks;
            private readonly int[] home, away;
            // +1 when the home team hosts, 0 at a neutral site
            private readonly int[] hosted;
            private readonly bool[] nonConference;
            private readonly bool[] conference;
            private readonly bool[] fcsGame;
            private readonly int[] movable;
            private readonly Dictionary<int, int[]> gamesByTeam;

            public Arrangement(PreseasonSchedule schedule, HashSet<ScheduledGame> locked)
            {
                games = schedule.Games.Where(g => !FcsTeams.Contains(g.HomeTeamRow) || !FcsTeams.Contains(g.AwayTeamRow)).ToList();
                originalWeeks = games.Select(g => g.Week).ToArray();
                weeks = games.Select(g => locked.Contains(g) ? g.Week : Math.Min(g.Week, LastScheduledWeek)).ToArray();
                home = games.Select(g => g.HomeTeamRow).ToArray();
                away = games.Select(g => g.AwayTeamRow).ToArray();
                hosted = games.Select(g => Site(schedule, g, g.HomeTeamRow)).ToArray();
                nonConference = games.Select(g => !IsConferenceGame(g) && !FcsTeams.Contains(g.HomeTeamRow) && !FcsTeams.Contains(g.AwayTeamRow)).ToArray();
                conference = games.Select(IsConferenceGame).ToArray();
                fcsGame = games.Select(g => FcsTeams.Contains(g.HomeTeamRow) || FcsTeams.Contains(g.AwayTeamRow)).ToArray();
                movable = Enumerable.Range(0, games.Count).Where(i => !locked.Contains(games[i])).ToArray();
                gamesByTeam = Enumerable.Range(0, games.Count)
                    .SelectMany(i => new[] { (Team: home[i], Game: i), (Team: away[i], Game: i) })
                    .Where(x => !FcsTeams.Contains(x.Team))
                    .GroupBy(x => x.Team)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Game).ToArray());
            }

            /// <summary>
            /// The search's starting point, built from scratch (as EADBEditor's MoveNonConfGamesEarly did): around the locked
            /// games, the non-conference games (other than FCS games; independents' first, since they have the most) take the
            /// earliest week both teams are open, then the conference games the latest from week 13 back to week 4 (earlier
            /// only when they must), and last each FCS game, which can go anywhere, takes the open week that best spaces its
            /// FBS team's byes. A game with no week both teams
            /// are open takes the week with the fewest clashes.
            /// </summary>
            private void Seed()
            {
                const int Weeks = LastScheduledWeek - FirstWeek + 1;
                var busy = gamesByTeam.Keys.ToDictionary(t => t, _ => new bool[LastScheduledWeek + 2]);
                bool Busy(int team, int week) => busy.TryGetValue(team, out var b) && b[week];
                void Book(int g)
                {
                    foreach (var team in new[] { home[g], away[g] })
                    {
                        if (busy.TryGetValue(team, out var b))
                        {
                            b[weeks[g]] = true;
                        }
                    }
                }

                var isMovable = new bool[games.Count];
                foreach (var g in movable)
                {
                    isMovable[g] = true;
                }

                for (int g = 0; g < games.Count; g++)
                {
                    if (!isMovable[g])
                    {
                        Book(g);
                    }
                }

                int NonConferenceGames(int team) => gamesByTeam.TryGetValue(team, out var list) ? list.Count(i => nonConference[i]) : 0;
                void Place(int g, bool late)
                {
                    // early: week 0 on; late: week 13 back to week 4, then the weeks before it
                    var order = late
                        ? Enumerable.Range(LastNonConferenceWeek, LastScheduledWeek - LastNonConferenceWeek + 1).Reverse()
                            .Concat(Enumerable.Range(FirstWeek, LastNonConferenceWeek - FirstWeek).Reverse())
                        : Enumerable.Range(FirstWeek, Weeks);
                    int week = order.FirstOrDefault(w => !Busy(home[g], w) && !Busy(away[g], w), -1);
                    weeks[g] = week >= 0 ? week
                        : order.OrderBy(w => (Busy(home[g], w) ? 1 : 0) + (Busy(away[g], w) ? 1 : 0)).First();
                    Book(g);
                }

                bool IsFcs(int g) => FcsTeams.Contains(home[g]) || FcsTeams.Contains(away[g]);

                foreach (var g in movable.Where(g => !conference[g] && !IsFcs(g))
                    .OrderByDescending(g => NonConferenceGames(home[g]) + NonConferenceGames(away[g])).ThenBy(g => g))
                {
                    Place(g, late: false);
                }

                foreach (var g in movable.Where(g => conference[g]))
                {
                    Place(g, late: true);
                }

                foreach (var g in movable.Where(IsFcs))
                {
                    int team = FcsTeams.Contains(home[g]) ? away[g] : home[g];
                    var open = Enumerable.Range(FirstWeek, Weeks).Where(w => !Busy(team, w)).ToList();
                    if (open.Count == 0)
                    {
                        Place(g, late: false);
                        continue;
                    }

                    // the open week whose use leaves the team's season cheapest
                    weeks[g] = open.OrderBy(w => { weeks[g] = w; return TeamCost(team); }).ThenBy(w => w).First();
                    Book(g);
                }
            }

            /// <summary>Arranges the games and writes their weeks; returns how many moved.</summary>
            public int Solve(Random random)
            {
                Seed();
                var isMovable = new bool[games.Count];
                foreach (var g in movable)
                {
                    isMovable[g] = true;
                }

                gamesPerWeek = new int[LastScheduledWeek + 2];
                foreach (var w in weeks)
                {
                    gamesPerWeek[w]++;
                }

                // a week should have at least this many games, so byes don't pile up in one week
                shortWeekTarget = games.Count * 85 / 100 / (LastScheduledWeek - FirstWeek + 1);

                int cost = gamesByTeam.Keys.Sum(TeamCost) + Enumerable.Range(0, games.Count).Sum(GameCost) +
                    Enumerable.Range(FirstWeek, LastScheduledWeek - FirstWeek + 1).Sum(ShortWeek);
                int bestCost = cost;
                var best = weeks.ToArray();
                // a move: up to three games and the weeks they had
                var moving = new List<int>(3);
                var oldWeeks = new List<int>(3);

                for (int step = 0; step < Iterations && movable.Length > 0; step++)
                {
                    double temperature = StartTemperature * Math.Pow(EndTemperature / StartTemperature, (double)step / Iterations);
                    int g = movable[random.Next(movable.Length)];
                    int from = weeks[g];
                    int to = random.Next(FirstWeek, LastScheduledWeek);
                    to = to >= from ? to + 1 : to;

                    moving.Clear();
                    moving.Add(g);
                    if (random.Next(2) == 1)
                    {
                        // and whatever either team plays in the new week takes the old one (a swap when only one does)
                        foreach (var team in new[] { home[g], away[g] })
                        {
                            if (!gamesByTeam.TryGetValue(team, out var teamGames))
                            {
                                continue;
                            }

                            foreach (var other in teamGames)
                            {
                                if (weeks[other] == to && !moving.Contains(other))
                                {
                                    moving.Add(other);
                                }
                            }
                        }

                        if (moving.Skip(1).Any(other => !isMovable[other]))
                        {
                            continue;
                        }
                    }

                    int before = Cost(moving) + ShortWeek(from) + ShortWeek(to);
                    oldWeeks.Clear();
                    foreach (var game in moving)
                    {
                        oldWeeks.Add(weeks[game]);
                        Move(game, game == g ? to : from);
                    }

                    int delta = Cost(moving) + ShortWeek(from) + ShortWeek(to) - before;
                    if (delta <= 0 || random.NextDouble() < Math.Exp(-delta / temperature))
                    {
                        cost += delta;
                        if (cost < bestCost)
                        {
                            bestCost = cost;
                            Array.Copy(weeks, best, weeks.Length);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < moving.Count; i++)
                        {
                            Move(moving[i], oldWeeks[i]);
                        }
                    }
                }

                int moved = 0;
                for (int i = 0; i < games.Count; i++)
                {
                    games[i].Week = best[i];
                    moved += best[i] != originalWeeks[i] ? 1 : 0;
                }

                return moved;
            }

            /// <summary>The cost the moving games' teams (each once) and the games themselves contribute.</summary>
            private int Cost(List<int> moving)
            {
                int cost = 0;
                Span<int> teams = stackalloc int[6];
                int count = 0;
                foreach (var g in moving)
                {
                    cost += GameCost(g);
                    foreach (var team in new[] { home[g], away[g] })
                    {
                        bool seen = false;
                        for (int i = 0; i < count; i++)
                        {
                            seen |= teams[i] == team;
                        }

                        if (!seen)
                        {
                            teams[count++] = team;
                            cost += TeamCostIfFbs(team);
                        }
                    }
                }

                return cost;
            }

            private int[] gamesPerWeek = Array.Empty<int>();
            private int shortWeekTarget;

            private void Move(int g, int week)
            {
                gamesPerWeek[weeks[g]]--;
                weeks[g] = week;
                gamesPerWeek[week]++;
            }

            /// <summary><see cref="ShortWeekCost"/> for each game a week is short of its share.</summary>
            private int ShortWeek(int week) => Math.Max(0, shortWeekTarget - gamesPerWeek[week]) * ShortWeekCost;

            private int TeamCostIfFbs(int team) => gamesByTeam.ContainsKey(team) ? TeamCost(team) : 0;

            private int GameCost(int g) =>
                (nonConference[g] ? Math.Max(0, weeks[g] - LastNonConferenceWeek) * LateNonConferenceCost : 0) +
                (conference[g] ? (weeks[g] == FirstWeek ? ConferenceWeekZeroCost : 0) + (LastScheduledWeek - weeks[g]) * EarlyConferenceCost +
                    (weeks[g] < LastNonConferenceWeek ? EarlyConferenceZoneCost : 0) : 0) +
                (fcsGame[g] ? (weeks[g] == LastScheduledWeek ? LastWeekFcsCost : 0) + Math.Max(0, weeks[g] - LastNonConferenceWeek) * LateFcsCost : 0);

            private int TeamCost(int team)
            {
                Span<int> perWeek = stackalloc int[LastScheduledWeek + 2];
                Span<int> site = stackalloc int[LastScheduledWeek + 2];
                foreach (var g in gamesByTeam[team])
                {
                    perWeek[weeks[g]]++;
                    site[weeks[g]] = hosted[g] == 0 ? 0 : home[g] == team ? 1 : -1;
                }

                return SeasonCost(perWeek, site);
            }
        }

        #region rules

        private const int DoubleBookedCost = 1000;
        private const int RuleCost = 40;
        // for a bye in the first or last week: each team's two byes belong in between
        private const int NoWeekZeroCost = 3;
        private const int NoLastWeekCost = 15;
        // byes a week apart rather than back to back: bye-game-bye (6-1-5) is fine
        private const int BackToBackByeCost = 12;
        // per week past LastNonConferenceWeek: a non-conference game in week 8 weighs as much as one broken rule
        private const int LateNonConferenceCost = 10;
        // more than a week-0 bye costs, so a conference game leaves week 0 when a non-conference or FCS game can take it
        private const int ConferenceWeekZeroCost = 8;
        // per week before week 13: conference games go as late as they can, FCS games fill in around them
        private const int EarlyConferenceCost = 1;
        // weeks 0-3 are for non-conference games; a conference game can slip into week 4
        private const int EarlyConferenceZoneCost = 40;
        // per game a week is short of 85% of an even share
        private const int ShortWeekCost = 4;
        // per week past week 4: an FCS game takes an early opening when its team has one, but can go late
        private const int LateFcsCost = 3;
        // an FCS game in week 13 is the last resort: more than a week-13 bye or any other preference, less than a broken rule
        private const int LastWeekFcsCost = 25;

        /// <summary>
        /// What a team's season breaks, from its games per week and the site of each week's game (+1 home, -1 road,
        /// 0 neutral): <see cref="DoubleBookedCost"/> for each extra game in a week, <see cref="RuleCost"/> for each game
        /// past <see cref="MaxGamesWithoutBye"/> in a row without a bye, each home game past <see cref="MaxHomeInARow"/>
        /// (<see cref="MaxOpeningHomeInARow"/> when they open the season) in a
        /// row and each road game past <see cref="MaxRoadInARow"/> (by game: a bye doesn't break a streak, a neutral site
        /// does), and
        /// <see cref="NoWeekZeroCost"/> for not playing in week 0, <see cref="NoLastWeekCost"/> for not playing in week 13 and
        /// <see cref="BackToBackByeCost"/> for byes in consecutive weeks.
        /// </summary>
        private static int SeasonCost(ReadOnlySpan<int> perWeek, ReadOnlySpan<int> site)
        {
            int cost = (perWeek[FirstWeek] == 0 ? NoWeekZeroCost : 0) + (perWeek[LastScheduledWeek] == 0 ? NoLastWeekCost : 0) +
                BackToBackByes(perWeek) * BackToBackByeCost;
            int run = 0, streak = 0, last = 0, played = 0;
            for (int week = 0; week < perWeek.Length; week++)
            {
                cost += Math.Max(0, perWeek[week] - 1) * DoubleBookedCost;
                run = perWeek[week] > 0 ? run + 1 : 0;
                cost += run > MaxGamesWithoutBye ? RuleCost : 0;

                if (perWeek[week] > 0)
                {
                    played++;
                    streak = site[week] == 0 ? 0 : site[week] == last ? streak + 1 : 1;
                    last = site[week];
                    cost += TooMany(streak, last, opensSeason: streak == played) ? RuleCost : 0;
                }
            }

            return cost;
        }

        /// <summary>Byes in consecutive weeks (weeks 0-13; week 14 is always empty).</summary>
        private static int BackToBackByes(ReadOnlySpan<int> perWeek)
        {
            int count = 0;
            for (int week = FirstWeek; week < LastScheduledWeek; week++)
            {
                count += perWeek[week] == 0 && perWeek[week + 1] == 0 ? 1 : 0;
            }

            return count;
        }

        /// <summary>Whether a streak of games at one site (+1 home, -1 road) is too long; a home stand that opens the season can be longer.</summary>
        private static bool TooMany(int streak, int site, bool opensSeason) =>
            streak > (site < 0 ? MaxRoadInARow : opensSeason ? MaxOpeningHomeInARow : MaxHomeInARow);

        /// <summary>What still breaks a rule, as report lines.</summary>
        private static IEnumerable<string> Summary(PreseasonSchedule schedule)
        {
            var doubleBooked = new List<string>();
            var noBye = new List<string>();
            var streaks = new List<string>();
            int noWeekZero = 0, noLastWeek = 0, backToBack = 0;

            var teams = schedule.Games.SelectMany(g => new[] { g.HomeTeamRow, g.AwayTeamRow }).Where(t => t >= 0 && !FcsTeams.Contains(t)).Distinct();
            foreach (var team in teams.OrderBy(t => schedule.TeamName(t)))
            {
                var season = schedule.GamesFor(team).OrderBy(g => g.Week).ToList();
                string name = schedule.TeamName(team) ?? team.ToString();
                int run = 0, streak = 0, last = 0, gamesSoFar = 0;
                bool tooLong = false, sameSite = false;
                for (int week = FirstWeek; week <= LastScheduledWeek + 1; week++)
                {
                    var played = season.Where(g => g.Week == week).ToList();
                    if (played.Count > 1) doubleBooked.Add($"{name} (week {week})");
                    run = played.Count > 0 ? run + 1 : 0;
                    tooLong |= run > MaxGamesWithoutBye;
                    foreach (var game in played)
                    {
                        int site = Site(schedule, game, team);
                        gamesSoFar++;
                        streak = site == 0 ? 0 : site == last ? streak + 1 : 1;
                        last = site;
                        sameSite |= TooMany(streak, last, opensSeason: streak == gamesSoFar);
                    }
                }

                if (tooLong) noBye.Add(name);
                if (sameSite) streaks.Add(name);
                if (!season.Any(g => g.Week == FirstWeek)) noWeekZero++;
                if (!season.Any(g => g.Week == LastScheduledWeek)) noLastWeek++;
                if (Enumerable.Range(FirstWeek, LastScheduledWeek - FirstWeek).Any(w => !season.Any(g => g.Week == w) && !season.Any(g => g.Week == w + 1))) backToBack++;
            }

            int lateNonConference = schedule.Games.Count(g => g.Week > LastNonConferenceWeek && !IsConferenceGame(g) &&
                !FcsTeams.Contains(g.HomeTeamRow) && !FcsTeams.Contains(g.AwayTeamRow));
            yield return $"{lateNonConference} non-conference games (not counting FCS games) after week {LastNonConferenceWeek}; {noWeekZero} teams don't play in week {FirstWeek}, {noLastWeek} in week {LastScheduledWeek}; {backToBack} have back-to-back byes.";
            if (doubleBooked.Count > 0) yield return $"Play twice in a week: {string.Join(", ", doubleBooked)}.";
            if (noBye.Count > 0) yield return $"More than {MaxGamesWithoutBye} games without a bye: {string.Join(", ", noBye)}.";
            if (streaks.Count > 0) yield return $"More than {MaxHomeInARow} home ({MaxOpeningHomeInARow} to open) or {MaxRoadInARow} road games in a row: {string.Join(", ", streaks)}.";
            var late = schedule.Games.Where(g => g.Week > LastScheduledWeek).ToList();
            if (late.Count > 0) yield return $"After week {LastScheduledWeek}: {string.Join(", ", late.Select(g => $"{Describe(schedule, g)} (week {g.Week})"))}.";
        }

        #endregion

        /// <summary>+1 when <paramref name="team"/> hosts, -1 on the road, 0 at a neutral site (a stadium that isn't the home team's).</summary>
        private static int Site(PreseasonSchedule schedule, ScheduledGame game, int team)
        {
            if (game.StadiumId != 0 && game.StadiumId != HomeStadium(schedule, game.HomeTeamRow))
            {
                return 0;
            }

            return game.HomeTeamRow == team ? 1 : -1;
        }

        private static uint HomeStadium(PreseasonSchedule schedule, int team) =>
            schedule.TeamTable.Records[team]["Stadium"] is string bits ? Convert.ToUInt32(bits, 2) : 0;

        private static bool IsP5(int team) => ScheduleHelpers.TeamAndConferences.TryGetValue(team, out var conference) && P5Conferences.Contains(conference);

        private static bool IsConferenceGame(ScheduledGame game) =>
            ScheduleHelpers.TeamAndConferences.TryGetValue(game.HomeTeamRow, out var home) &&
            ScheduleHelpers.TeamAndConferences.TryGetValue(game.AwayTeamRow, out var away) &&
            home == away && (home != ScheduleHelpers.Independent ||
                (independentConference.Contains(game.HomeTeamRow) && independentConference.Contains(game.AwayTeamRow)));

        private static string Describe(PreseasonSchedule schedule, ScheduledGame game) =>
            $"{schedule.TeamName(game.AwayTeamRow)} at {schedule.TeamName(game.HomeTeamRow)}";

        private static (int, int) Pair(int a, int b) => a < b ? (a, b) : (b, a);
    }
}

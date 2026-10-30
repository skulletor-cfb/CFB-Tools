using CFB27.Data.Model;
using MaddenFranchise;
using MaddenFranchise.Dynasty;
using NCAA14Utility.Scheduling;

namespace ScheduleMaker
{
    /// <summary>
    /// Ctrl+G. Evens out G5 schedules by trading opponents between two games in the same week, so no game moves and no day
    /// changes:
    /// 1. a team with more than one FCS opponent gives one to a team with none (a P5 team if there is one);
    /// 2. a G5 team with more than two P5 opponents gives a P5 game to a G5 team with fewer than two, preferring the
    ///    P5 conference's G5 partners (<see cref="Affinities"/>).
    /// A game can change only if it's on Saturday, isn't a conference game, a rivalry (the save's Rivalry table), P5 vs
    /// P5, a P5 team away at a G5 team, a neutral-site or kickoff game, and doesn't involve an independent, Air Force,
    /// Navy, Army or Hawai'i. Every game it makes is a non-conference game, with a P5 team hosting a G5 team and an FBS
    /// team hosting an FCS team.
    /// </summary>
    internal sealed class G5Smoother
    {
        private const string Saturday = "Saturday";
        private const int MaxFcsOpponents = 1;
        private const int MaxP5Opponents = 2;
        private const int SunBeltEast = 17;

        private static readonly HashSet<int> FcsTeams = new HashSet<int>
        {
            ScheduleBase.FCSEast, ScheduleBase.FCSMidwest, ScheduleBase.FCSNorthwest, ScheduleBase.FCSSoutheast, ScheduleBase.FCSWest,
        };

        private static readonly HashSet<int> P5Conferences = new HashSet<int>
        {
            ScheduleHelpers.ACC, ScheduleHelpers.Big10, ScheduleHelpers.Big12, ScheduleHelpers.Pac12, ScheduleHelpers.SEC,
        };

        private static readonly HashSet<int> FrozenTeams = new HashSet<int>
        {
            ScheduleBase.AF, ScheduleBase.Navy, ScheduleBase.Army, ScheduleBase.Hawaii,
        };

        /// <summary>
        /// The G5 conferences each P5 conference schedules (Sun Belt East only for the ACC): Big Ten with the MAC, Pac-12
        /// with the MWC, ACC with Sun Belt East and the American, Big 12 and SEC with the Sun Belt, C-USA and the American.
        /// </summary>
        private static readonly Dictionary<int, int[]> Affinities = new Dictionary<int, int[]>
        {
            [ScheduleHelpers.Big10] = new[] { ScheduleHelpers.MAC },
            [ScheduleHelpers.Pac12] = new[] { ScheduleHelpers.MWC },
            [ScheduleHelpers.ACC] = new[] { ScheduleHelpers.SBC, ScheduleHelpers.American },
            [ScheduleHelpers.Big12] = new[] { ScheduleHelpers.SBC, ScheduleHelpers.CUSA, ScheduleHelpers.American },
            [ScheduleHelpers.SEC] = new[] { ScheduleHelpers.SBC, ScheduleHelpers.CUSA, ScheduleHelpers.American },
        };

        private readonly PreseasonSchedule schedule;
        private readonly HashSet<(int, int)> rivalries;
        private readonly Dictionary<int, int> divisionByTeam;
        private readonly List<string> changes = new List<string>();

        public G5Smoother(FranchiseFile dynasty, PreseasonSchedule schedule)
        {
            this.schedule = schedule;
            rivalries = SeasonInitializer.ReadRivalries(dynasty, schedule);
            divisionByTeam = CFBConference.Read(dynasty)
                .SelectMany(c => c.Divisions.SelectMany(d => d.TeamRows.Select(row => (Row: row, d.ClassicId))))
                .GroupBy(x => x.Row)
                .ToDictionary(g => g.Key, g => g.First().ClassicId);
        }

        /// <summary>Runs both steps. Returns a report: a line per step, then each change.</summary>
        public List<string> Smooth()
        {
            int fcsSwaps = SpreadFcsGames();
            int p5Swaps = SpreadP5Games();
            var report = new List<string>
            {
                $"{fcsSwaps} FCS games moved from teams with more than {MaxFcsOpponents} FCS opponent to teams with none.",
                $"{p5Swaps} P5 games moved from G5 teams with more than {MaxP5Opponents} P5 opponents to G5 teams with fewer.",
            };

            int stillFcs = Teams().Count(t => FcsOpponents(t) > MaxFcsOpponents);
            int stillP5 = Teams().Count(t => IsG5(t) && P5Opponents(t) > MaxP5Opponents);
            if (stillFcs + stillP5 > 0)
            {
                report.Add($"Couldn't be evened out within these rules: {stillFcs} teams with more than {MaxFcsOpponents} FCS opponent, " +
                    $"{stillP5} G5 teams with more than {MaxP5Opponents} P5 opponents.");
            }

            report.AddRange(changes);
            return report;
        }

        /// <summary>Step 1: each extra FCS game goes, in its week, to a team with no FCS opponent (P5 first).</summary>
        private int SpreadFcsGames()
        {
            int swaps = 0;
            foreach (var team in Teams().Where(t => FcsOpponents(t) > MaxFcsOpponents).OrderByDescending(FcsOpponents).ToList())
            {
                foreach (var fcsGame in schedule.GamesFor(team).Where(g => IsFcsGame(g) && Changeable(g)).OrderByDescending(g => g.Week).ToList())
                {
                    if (FcsOpponents(team) <= MaxFcsOpponents)
                    {
                        break;
                    }

                    int fcs = Opponent(fcsGame, team);
                    // the receiver gets the FCS team at home; the team takes the receiver's opponent
                    var swap = schedule.Games
                        .Where(g => g != fcsGame && g.Week == fcsGame.Week && Changeable(g) && !IsFcsGame(g))
                        .SelectMany(g => new[] { g.HomeTeamRow, g.AwayTeamRow }.Select(r => (Game: g, Receiver: r, Other: Opponent(g, r))))
                        .Where(s => s.Receiver != team && s.Other != team && FcsOpponents(s.Receiver) == 0 &&
                            NonConference(team, s.Other) && Allowed(team, s.Other))
                        .OrderBy(s => IsP5(s.Receiver) ? 0 : 1)
                        .ThenBy(s => s.Game.Week)
                        .FirstOrDefault();
                    if (swap.Game == null)
                    {
                        continue;
                    }

                    string before = $"{Describe(fcsGame)}, {Describe(swap.Game)}";
                    Set(swap.Game, swap.Receiver, fcs);
                    Set(fcsGame, team, swap.Other);
                    changes.Add($"  week {fcsGame.Week}: {before} -> {Describe(swap.Game)}, {Describe(fcsGame)}");
                    swaps++;
                }
            }

            return swaps;
        }

        /// <summary>
        /// Step 2: a G5 team with too many P5 opponents gives one, in its week, to a G5 team with fewer than two, which
        /// gives up its G5 opponent in return. Partners in the P5 conference's affinity come first.
        /// </summary>
        private int SpreadP5Games()
        {
            int swaps = 0;
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var team in Teams().Where(t => IsG5(t) && P5Opponents(t) > MaxP5Opponents).OrderByDescending(P5Opponents).ToList())
                {
                    // give away the P5 games that fit the team's conference worst first
                    foreach (var p5Game in schedule.GamesFor(team)
                        .Where(g => IsP5(Opponent(g, team)) && Changeable(g))
                        .OrderBy(g => Fits(Opponent(g, team), team) ? 1 : 0)
                        .ToList())
                    {
                        if (P5Opponents(team) <= MaxP5Opponents)
                        {
                            break;
                        }

                        int p5 = Opponent(p5Game, team);
                        var swap = schedule.Games
                            .Where(g => g != p5Game && g.Week == p5Game.Week && Changeable(g) && !IsFcsGame(g))
                            .SelectMany(g => new[] { g.HomeTeamRow, g.AwayTeamRow }.Select(r => (Game: g, Taker: r, Other: Opponent(g, r))))
                            .Where(s => s.Taker != team && s.Other != team && IsG5(s.Taker) && !IsP5(s.Other) && IsFbs(s.Other) &&
                                P5Opponents(s.Taker) < MaxP5Opponents && NonConference(team, s.Other) && NonConference(p5, s.Taker) &&
                                Allowed(team, s.Other) && Allowed(p5, s.Taker))
                            .OrderBy(s => Fits(p5, s.Taker) ? 0 : 1)
                            .ThenBy(s => P5Opponents(s.Taker))
                            .FirstOrDefault();
                        if (swap.Game == null)
                        {
                            continue;
                        }

                        string before = $"{Describe(p5Game)}, {Describe(swap.Game)}";
                        Set(p5Game, p5, swap.Taker);
                        Set(swap.Game, team, swap.Other, keepHomeOf: swap.Game.HomeTeamRow == swap.Taker ? team : swap.Other);
                        changes.Add($"  week {p5Game.Week}: {before} -> {Describe(p5Game)}, {Describe(swap.Game)}");
                        swaps++;
                        changed = true;
                    }
                }
            }

            return swaps;
        }

        /// <summary>
        /// Makes the game <paramref name="a"/> vs <paramref name="b"/>: a P5 team hosts a G5 team and an FBS team hosts an
        /// FCS team; otherwise <paramref name="keepHomeOf"/> (or <paramref name="a"/>) hosts.
        /// </summary>
        private static void Set(ScheduledGame game, int a, int b, int? keepHomeOf = null)
        {
            int home = FcsTeams.Contains(b) ? a
                : FcsTeams.Contains(a) ? b
                : IsP5(a) && !IsP5(b) ? a
                : IsP5(b) && !IsP5(a) ? b
                : keepHomeOf ?? a;
            game.SetHomeTeam(home);
            game.SetAwayTeam(home == a ? b : a);
        }

        private bool Changeable(ScheduledGame g)
        {
            int home = g.HomeTeamRow, away = g.AwayTeamRow;
            return g.DayOfWeek == Saturday && g.IsEnabled && g.StadiumId == 0 && !g.IsKickoffGame &&
                !FrozenTeams.Contains(home) && !FrozenTeams.Contains(away) && !IsIndependent(home) && !IsIndependent(away) &&
                !(IsP5(home) && IsP5(away)) && !(IsP5(away) && IsG5(home)) &&
                !rivalries.Contains(Pair(home, away)) && NonConference(home, away);
        }

        /// <summary>A pair the smoother may create: no frozen team, no independent, not P5 vs P5.</summary>
        private static bool Allowed(int a, int b) =>
            !FrozenTeams.Contains(a) && !FrozenTeams.Contains(b) && !IsIndependent(a) && !IsIndependent(b) && !(IsP5(a) && IsP5(b));

        /// <summary>Whether a G5 team is one of the P5 team's conference partners.</summary>
        private bool Fits(int p5, int g5) =>
            ScheduleHelpers.TeamAndConferences.TryGetValue(p5, out var p5Conference) &&
            ScheduleHelpers.TeamAndConferences.TryGetValue(g5, out var g5Conference) &&
            Affinities.TryGetValue(p5Conference, out var partners) && partners.Contains(g5Conference) &&
            (p5Conference != ScheduleHelpers.ACC || g5Conference != ScheduleHelpers.SBC || divisionByTeam.GetValueOrDefault(g5) == SunBeltEast);

        private IEnumerable<int> Teams() =>
            schedule.Games.SelectMany(g => new[] { g.HomeTeamRow, g.AwayTeamRow }).Where(t => IsFbs(t) && !FrozenTeams.Contains(t) && !IsIndependent(t)).Distinct();

        private int FcsOpponents(int team) => schedule.GamesFor(team).Count(IsFcsGame);

        private int P5Opponents(int team) => schedule.GamesFor(team).Count(g => IsP5(Opponent(g, team)));

        private static bool IsFcsGame(ScheduledGame g) => FcsTeams.Contains(g.HomeTeamRow) || FcsTeams.Contains(g.AwayTeamRow);

        private static bool IsFbs(int team) => ScheduleHelpers.TeamAndConferences.ContainsKey(team);

        private static bool IsP5(int team) => ScheduleHelpers.TeamAndConferences.TryGetValue(team, out var c) && P5Conferences.Contains(c);

        private static bool IsIndependent(int team) => ScheduleHelpers.TeamAndConferences.TryGetValue(team, out var c) && c == ScheduleHelpers.Independent;

        private static bool IsG5(int team) => IsFbs(team) && !IsP5(team) && !IsIndependent(team);

        /// <summary>Different conferences (FCS teams have none).</summary>
        private static bool NonConference(int a, int b) =>
            a != b && !(ScheduleHelpers.TeamAndConferences.TryGetValue(a, out var ca) && ScheduleHelpers.TeamAndConferences.TryGetValue(b, out var cb) && ca == cb);

        private static int Opponent(ScheduledGame g, int team) => g.HomeTeamRow == team ? g.AwayTeamRow : g.HomeTeamRow;

        private string Describe(ScheduledGame g) => $"{schedule.TeamName(g.AwayTeamRow)} at {schedule.TeamName(g.HomeTeamRow)}";

        private static (int, int) Pair(int a, int b) => a < b ? (a, b) : (b, a);
    }
}

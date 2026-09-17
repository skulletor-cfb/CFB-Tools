using System;
using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor
{
    /// <summary>
    /// Result of a <see cref="ScheduleRuleEnforcer.Enforce"/> pass. A schedule can always be
    /// left in a state that still breaks a rule when the only games available to move are
    /// themselves locked to their week - <see cref="RemainingViolations"/> is how callers find
    /// out that happened instead of silently shipping a bad schedule.
    /// </summary>
    public class ScheduleEnforcementResult
    {
        /// <summary>Notes logged while a fix was attempted but blocked by a conference lock.</summary>
        public List<string> UnresolvedNotes { get; } = new List<string>();

        /// <summary>Violations that are still present after the enforcer gave up.</summary>
        public List<string> RemainingViolations { get; } = new List<string>();

        public bool Success => RemainingViolations.Count == 0;
    }

    /// <summary>
    /// Repairs an already-generated schedule so it satisfies three rules, without ever
    /// changing which teams play each other and without ever changing which team hosts a
    /// given game:
    ///   1. A game locked to a week by <see cref="ConferenceLocks"/> is never moved.
    ///   2. No team plays 3 or more consecutive conference games away from home.
    ///   3. No team goes 7 or more consecutive calendar weeks without a bye.
    /// Both rules are fixed the same way: by relocating a movable game (never a locked one) to
    /// a different week. First choice is a week both teams already have open. When no such week
    /// exists (most teams only have one bye), it falls back to swapping weeks between the game
    /// and one of the team's own other games - matchups and home teams stay exactly as they
    /// were, only which week each is played in changes. Every candidate move is re-verified
    /// against both rules for every team it touches before being kept, so a fix for one team
    /// can't quietly break the same rule for its opponent.
    /// </summary>
    public static class ScheduleRuleEnforcer
    {
        public const int MaxConsecutiveConferenceRoadGames = 2;
        public const int MaxConsecutiveWeeksWithoutBye = 6;

        private static readonly Dictionary<int, Func<ConferenceLocks>> LockFactoriesByConference = new Dictionary<int, Func<ConferenceLocks>>
        {
            { TableUtility.SECId, () => new SecLocks() },
            { TableUtility.ACCId, () => new AccLocks() },
            { TableUtility.Big10Id, () => new Big10Locks() },
            { TableUtility.Big12Id, () => new Big12Locks() },
            { TableUtility.Pac16Id, () => new Pac12Locks() },
            { TableUtility.MACId, () => new MACLocks() },
            { TableUtility.MWCId, () => new MWCLocks() },
            { TableUtility.CUSAId, () => new CUSALocks() },
            { TableUtility.AmericanId, () => new AmericanLocks() },
            { TableUtility.SBCId, () => new SunBeltLocks() },
        };

        private static readonly Dictionary<int, ConferenceLocks> lockInstances = new Dictionary<int, ConferenceLocks>();

        private static ConferenceLocks GetLocksFor(int confId)
        {
            if (lockInstances.TryGetValue(confId, out var cached))
                return cached;

            if (!LockFactoriesByConference.TryGetValue(confId, out var factory))
                return null;

            var instance = factory();
            lockInstances[confId] = instance;
            return instance;
        }

        /// <summary>True when this matchup is pinned to a specific week and must not be touched.</summary>
        public static bool IsLocked(PreseasonScheduledGame game)
        {
            if (game == null || !game.IsConferenceGame())
                return false;

            var locks = GetLocksFor(game.ConferenceGameId().Value);
            return locks != null && locks.CheckWeekLock(game).HasValue;
        }

        /// <summary>
        /// Runs both fixups to a fixed point (a fix for one team can create a new violation for
        /// its opponent, so this repeats until nothing changes or <paramref name="maxIterations"/>
        /// is hit) and returns whatever could not be resolved.
        /// </summary>
        public static ScheduleEnforcementResult Enforce(Dictionary<int, TeamSchedule> schedules, int maxIterations = 25)
        {
            var result = new ScheduleEnforcementResult();

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                var changed = false;

                changed |= FixConsecutiveConferenceRoadGames(schedules, result);
                changed |= FixLongStreaksWithoutABye(schedules, result);

                if (!changed)
                    break;

                // notes logged mid-run may get resolved by a later iteration - only the notes
                // from the final, no-progress pass are worth keeping.
                result.UnresolvedNotes.Clear();
            }

            CollectRemainingViolations(schedules, result);
            return result;
        }

        // ---- Rule 2: no 3+ straight conference road games -------------------------------

        private static bool FixConsecutiveConferenceRoadGames(Dictionary<int, TeamSchedule> schedules, ScheduleEnforcementResult result)
        {
            var changedAny = false;

            foreach (var teamId in schedules.Keys.ToArray())
            {
                if (teamId.IsFcsTeam())
                    continue;

                var schedule = schedules[teamId];
                var roadRun = new List<PreseasonScheduledGame>();

                for (int week = 0; week < TeamSchedule.ScheduleLimit; week++)
                {
                    var game = schedule[week];

                    // byes and non-conference games don't break or extend a conference road streak
                    if (game == null || !game.IsConferenceGame())
                        continue;

                    if (game.AwayTeam != teamId)
                    {
                        roadRun.Clear();
                        continue;
                    }

                    roadRun.Add(game);

                    if (roadRun.Count <= MaxConsecutiveConferenceRoadGames)
                        continue;

                    if (TryRelocateOneGameOutOfRun(schedules, teamId, roadRun))
                    {
                        changedAny = true;
                        roadRun.Clear();
                    }
                    // else: nothing in the run so far could be moved - keep extending and try
                    // again next week, in case a later game in the run is movable.
                }

                if (roadRun.Count > MaxConsecutiveConferenceRoadGames)
                {
                    var last = roadRun[roadRun.Count - 1];
                    result.UnresolvedNotes.Add(
                        $"Team {teamId} has {roadRun.Count} straight road conference games through week {last.Week} " +
                        $"(vs {last.OpponentId(teamId)}) - no game in that stretch could be relocated.");
                }
            }

            return changedAny;
        }

        private static bool TryRelocateOneGameOutOfRun(Dictionary<int, TeamSchedule> schedules, int teamId, List<PreseasonScheduledGame> run)
        {
            // try the most recently added game in the run first, then walk backward
            for (int i = run.Count - 1; i >= 0; i--)
            {
                var game = run[i];

                if (IsLocked(game))
                    continue;

                if (TryRelocateGameToMutualOpenWeek(schedules, game, teamId))
                    return true;

                if (TrySwapWeekWithAnotherGame(schedules, game, teamId))
                    return true;
            }

            return false;
        }

        // ---- Rule 3: no 7+ straight weeks without a bye ----------------------------------

        private static bool FixLongStreaksWithoutABye(Dictionary<int, TeamSchedule> schedules, ScheduleEnforcementResult result)
        {
            var changedAny = false;

            foreach (var teamId in schedules.Keys.ToArray())
            {
                if (teamId.IsFcsTeam())
                    continue;

                // moving one game can shift where the "worst" streak is, so keep re-scanning
                // this team until nothing is left to fix or a fix attempt fails outright.
                while (true)
                {
                    var run = FindFirstLongPlayingStreak(schedules[teamId]);

                    if (run == null)
                        break;

                    if (!TryOpenByeWithinStreak(schedules, teamId, run.Value.start, run.Value.end, result))
                        break;

                    changedAny = true;
                }
            }

            return changedAny;
        }

        private static (int start, int end)? FindFirstLongPlayingStreak(TeamSchedule schedule)
        {
            var runStart = -1;

            for (int week = 0; week <= TeamSchedule.ScheduleLimit; week++)
            {
                var hasGame = week < TeamSchedule.ScheduleLimit && schedule[week] != null;

                if (hasGame)
                {
                    if (runStart == -1)
                        runStart = week;

                    if (week - runStart + 1 > MaxConsecutiveWeeksWithoutBye)
                        return (runStart, week);
                }
                else
                {
                    runStart = -1;
                }
            }

            return null;
        }

        private static bool TryOpenByeWithinStreak(Dictionary<int, TeamSchedule> schedules, int teamId, int start, int end, ScheduleEnforcementResult result)
        {
            var schedule = schedules[teamId];
            var middle = (start + end) / 2;

            // prefer moving a game near the middle of the streak (splits it most evenly) and
            // prefer non-rivalry games (rivalry weeks are usually meaningful to keep in place)
            var candidates = Enumerable.Range(start, end - start + 1)
                .Select(week => schedule[week])
                .Where(g => g != null && !IsLocked(g))
                .OrderBy(g => Math.Abs(g.WeekIndex - middle))
                .ThenBy(g => g.IsRivalryGame() ? 1 : 0)
                .ToList();

            foreach (var game in candidates)
            {
                if (TryRelocateGameToMutualOpenWeek(schedules, game, teamId))
                    return true;

                if (TrySwapWeekWithAnotherGame(schedules, game, teamId))
                    return true;
            }

            result.UnresolvedNotes.Add(
                $"Team {teamId} plays weeks {start + 1}-{end + 1} straight with no bye, and no game in that " +
                "stretch could be relocated without breaking a lock or another team's rule.");
            return false;
        }

        // ---- shared move primitives --------------------------------------------------------

        /// <summary>
        /// Moves <paramref name="game"/> to a week that is currently open for both <paramref
        /// name="teamId"/> and its opponent, if one exists, keeping the move only if it doesn't
        /// break either rule for either team. Home team and matchup are untouched.
        /// </summary>
        private static bool TryRelocateGameToMutualOpenWeek(Dictionary<int, TeamSchedule> schedules, PreseasonScheduledGame game, int teamId)
        {
            var opponentId = game.OpponentId(teamId);

            if (!schedules.TryGetValue(opponentId, out var opponentSchedule))
                return false; // e.g. an FCS opponent has no independent slate to shuffle into

            var schedule = schedules[teamId];
            var originalWeek = game.WeekIndex;
            var candidateWeeks = schedule.FindOpenWeeks().Intersect(opponentSchedule.FindOpenWeeks()).OrderBy(w => w).ToList();

            foreach (var candidateWeek in candidateWeeks)
            {
                MoveGame(schedules, game, candidateWeek);

                if (IsSafe(schedule, teamId) && IsSafe(opponentSchedule, opponentId))
                    return true;

                MoveGame(schedules, game, originalWeek);
            }

            return false;
        }

        /// <summary>
        /// Fallback for when no mutual open week exists (the common case - most teams only have
        /// one bye): swap <paramref name="game"/>'s week with one of <paramref name="teamId"/>'s
        /// own other games. Both opponents keep their original home team and matchup, they just
        /// trade which week they play <paramref name="teamId"/>. Only kept if it doesn't break
        /// either rule for any of the three teams involved.
        /// A real non-conference FBS opponent has its own schedule commitments and is never
        /// disturbed just to make room here - only conference games and non-conference games
        /// against an FCS opponent are eligible to be the "other" game that gets bumped.
        /// </summary>
        private static bool TrySwapWeekWithAnotherGame(Dictionary<int, TeamSchedule> schedules, PreseasonScheduledGame game, int teamId)
        {
            var opponentId = game.OpponentId(teamId);

            if (!schedules.TryGetValue(opponentId, out var opponentSchedule))
                return false;

            var schedule = schedules[teamId];
            var originalWeek = game.WeekIndex;

            var otherGames = Enumerable.Range(0, TeamSchedule.ScheduleLimit)
                .Select(week => schedule[week])
                .Where(g => g != null && g != game && !IsLocked(g) && IsEligibleToBeBumped(g, teamId))
                .ToList();

            foreach (var other in otherGames)
            {
                var otherOpponentId = other.OpponentId(teamId);

                if (!schedules.TryGetValue(otherOpponentId, out var otherOpponentSchedule))
                    continue;

                var otherWeek = other.WeekIndex;

                // each opponent needs to actually be free on the week they'd be inheriting
                if (opponentSchedule[otherWeek] != null || otherOpponentSchedule[originalWeek] != null)
                    continue;

                SwapGameWeeks(schedules, game, other);

                if (IsSafe(schedule, teamId) && IsSafe(opponentSchedule, opponentId) && IsSafe(otherOpponentSchedule, otherOpponentId))
                    return true;

                SwapGameWeeks(schedules, game, other); // swapping back is its own inverse
            }

            return false;
        }

        /// <summary>
        /// True if this game is safe to bump to a different week purely to make room for
        /// another fix: it's either a conference game (already ours to schedule) or a
        /// non-conference game against an FCS opponent (a scheduling formality, not a
        /// real FBS opponent's own commitment).
        /// </summary>
        private static bool IsEligibleToBeBumped(PreseasonScheduledGame game, int teamId)
        {
            return game.IsConferenceGame() || game.OpponentId(teamId).IsFcsTeam();
        }

        private static void MoveGame(Dictionary<int, TeamSchedule> schedules, PreseasonScheduledGame game, int newWeek)
        {
            var oldWeek = game.WeekIndex;
            schedules[game.HomeTeam][oldWeek] = null;
            schedules[game.AwayTeam][oldWeek] = null;
            game.SetWeek(newWeek);
            schedules[game.HomeTeam][newWeek] = game;
            schedules[game.AwayTeam][newWeek] = game;
        }

        private static void SwapGameWeeks(Dictionary<int, TeamSchedule> schedules, PreseasonScheduledGame a, PreseasonScheduledGame b)
        {
            var weekA = a.WeekIndex;
            var weekB = b.WeekIndex;

            schedules[a.HomeTeam][weekA] = null;
            schedules[a.AwayTeam][weekA] = null;
            schedules[b.HomeTeam][weekB] = null;
            schedules[b.AwayTeam][weekB] = null;

            a.SetWeek(weekB);
            b.SetWeek(weekA);

            schedules[a.HomeTeam][a.WeekIndex] = a;
            schedules[a.AwayTeam][a.WeekIndex] = a;
            schedules[b.HomeTeam][b.WeekIndex] = b;
            schedules[b.AwayTeam][b.WeekIndex] = b;
        }

        private static bool IsSafe(TeamSchedule schedule, int teamId)
        {
            return GetMaxConsecutiveConferenceRoadStreak(schedule, teamId) <= MaxConsecutiveConferenceRoadGames &&
                   GetMaxWeeksWithoutBye(schedule) <= MaxConsecutiveWeeksWithoutBye;
        }

        // ---- shared measurement + final report -------------------------------------------

        private static int GetMaxConsecutiveConferenceRoadStreak(TeamSchedule schedule, int teamId)
        {
            var max = 0;
            var current = 0;

            for (int week = 0; week < TeamSchedule.ScheduleLimit; week++)
            {
                var game = schedule[week];

                if (game == null || !game.IsConferenceGame())
                    continue;

                if (game.AwayTeam == teamId)
                {
                    current++;
                    max = Math.Max(max, current);
                }
                else
                {
                    current = 0;
                }
            }

            return max;
        }

        private static int GetMaxWeeksWithoutBye(TeamSchedule schedule)
        {
            var max = 0;
            var current = 0;

            for (int week = 0; week < TeamSchedule.ScheduleLimit; week++)
            {
                if (schedule[week] != null)
                {
                    current++;
                    max = Math.Max(max, current);
                }
                else
                {
                    current = 0;
                }
            }

            return max;
        }

        private static void CollectRemainingViolations(Dictionary<int, TeamSchedule> schedules, ScheduleEnforcementResult result)
        {
            foreach (var kvp in schedules)
            {
                var teamId = kvp.Key;

                if (teamId.IsFcsTeam())
                    continue;

                var roadStreak = GetMaxConsecutiveConferenceRoadStreak(kvp.Value, teamId);
                if (roadStreak > MaxConsecutiveConferenceRoadGames)
                    result.RemainingViolations.Add($"Team {teamId}: {roadStreak} consecutive road conference games.");

                var noByeStreak = GetMaxWeeksWithoutBye(kvp.Value);
                if (noByeStreak > MaxConsecutiveWeeksWithoutBye)
                    result.RemainingViolations.Add($"Team {teamId}: {noByeStreak} consecutive weeks without a bye.");
            }
        }
    }
}

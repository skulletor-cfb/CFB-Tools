#nullable disable
using NCAA14Utility.Scheduling;

namespace ScheduleMaker.TV
{
    public static class TelevisionHelper
    {
        public static TelevisedGameQueue ToQueue(this IEnumerable<TelevisedGame> items, int? take = null)
        {
            if (take.HasValue)
            {
                items = items.Take(take.Value);
            }

            return new TelevisedGameQueue(items);
        }

        public static Dictionary<int, List<TelevisedGame>> GetAvailableGamesByWeek(
            this List<TelevisedGame> games,
            Func<TelevisedGame, int> orderFunc = null,
            Func<TelevisedGame, bool> selector = null
            )
        {
            selector = selector ?? (tvg => !tvg.Selected);
            orderFunc = orderFunc ?? (tvg => tvg.Score);
            return games.GroupBy(g => g.Week)
                .ToDictionary(g => g.Key, g => g.Where(game => selector(game)).OrderBy(game => orderFunc(game)).ToList());
        }

        /// <summary>The week's games, or none (NCAA 14 always had games for every network every week; a CFB 27 season may not).</summary>
        public static List<TelevisedGame> Week(this Dictionary<int, List<TelevisedGame>> weeks, int week) =>
            weeks.TryGetValue(week, out var games) ? games : new List<TelevisedGame>();

        /// <summary>
        /// given a list of games get the best games in DIFFERENT weeks
        /// </summary>
        public static List<TelevisedGame> Draft(this IEnumerable<TelevisedGame> games, int take)
        {
            var result = new Dictionary<int, TelevisedGame>();

            foreach (var game in games)
            {
                // already in this week
                if (game.Selected || result.ContainsKey(game.Week))
                {
                    continue;
                }

                result[game.Week] = game;

                if (result.Count == take)
                {
                    break;
                }
            }

            return result.Values.ToList();
        }

        /// <summary>
        /// given a list of games, choose N of them in order
        /// </summary>
        public static List<TelevisedGame> Choose(this IEnumerable<TelevisedGame> games, int take)
        {
            var result = new List<TelevisedGame>();

            foreach (var game in games)
            {
                // already in this week
                if (game.Selected)
                {
                    continue;
                }

                result.Add(game);

                if (result.Count == take)
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>Shuffles the array in place with the run's random numbers (seeded by the season, so a rerun matches).</summary>
        public static T[] Shuffle<T>(this T[] array)
        {
            int n = array.Length;
            for (int i = 0; i < (n - 1); i++)
            {
                int r = i + TelevisionScheduler.Random.Next(n - i);
                (array[r], array[i]) = (array[i], array[r]);
            }

            return array;
        }

        public static T[] CreateAndShuffle<T>(this T[] array) => ((T[])array.Clone()).Shuffle();

        public static DateTime GetDate(this TelevisedGame game) => TelevisionScheduler.CurrentSeason.Weeks[game.Week].AddDays(TimeSlot.DaysFromSaturday(game.Day));

        public static bool IsOctober(this int week) => TelevisionScheduler.CurrentSeason.IsOctober(week);

        public static bool IsAugustSeptember(this int week) => TelevisionScheduler.CurrentSeason.IsAugustSeptember(week);

        public static bool IsNovember(this int week) => TelevisionScheduler.CurrentSeason.IsNovember(week);
    }

    /// <summary>Team questions the TV code asks, by Team row, from the save's conferences (<see cref="ScheduleHelpers.TeamAndConferences"/>).</summary>
    public static class TvTeams
    {
        private static readonly HashSet<int> P5 = new HashSet<int> { ScheduleHelpers.ACC, ScheduleHelpers.Big10, ScheduleHelpers.Big12, ScheduleHelpers.Pac12, ScheduleHelpers.SEC };
        private static readonly HashSet<int> G5 = new HashSet<int> { ScheduleHelpers.American, ScheduleHelpers.CUSA, ScheduleHelpers.MAC, ScheduleHelpers.MWC, ScheduleHelpers.SBC };

        private static int? Conference(int team) => ScheduleHelpers.TeamAndConferences.TryGetValue(team, out var c) ? c : null;

        /// <summary>The generic FCS teams: anything outside the conferences.</summary>
        public static bool IsFcsTeam(this int team) => Conference(team) == null;

        public static bool IsP5(this int team) => Conference(team) is int c && P5.Contains(c);

        public static bool IsP5OrND(this int team) => team.IsP5() || team == ScheduleBase.NotreDame;

        public static bool IsG5(this int team) =>
            (Conference(team) is int c && G5.Contains(c)) || team == ScheduleBase.Navy || team == ScheduleBase.Army || team == ScheduleBase.AF;

        public static bool IsIndependentND(this int team) => team == ScheduleBase.NotreDame && Conference(team) == ScheduleHelpers.Independent;

        public static bool IsSECTeam(this int team) => Conference(team) == ScheduleHelpers.SEC;

        public static bool IsAccTeam(this int team) => Conference(team) == ScheduleHelpers.ACC;

        public static bool IsBig10Team(this int team) => Conference(team) == ScheduleHelpers.Big10;

        public static bool IsPac12Team(this int team) => Conference(team) == ScheduleHelpers.Pac12;

        public static bool IsSunBeltTeam(this int team) => Conference(team) == ScheduleHelpers.SBC;

        public static bool TeamsInSameConference(int a, int b) => Conference(a) is int c && Conference(b) == c;

        /// <summary>The home team's conference (the visitor's when an FCS team hosts).</summary>
        public static int GameConferenceOwner(int homeTeam, int awayTeam) => Conference(homeTeam) ?? Conference(awayTeam) ?? ScheduleHelpers.Independent;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor.Scheduling
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

        /// <summary>
        /// given a list of games get the best games in DIFFERENT weeks
        /// </summary>
        /// <param name="games"></param>
        /// <param name="take"></param>
        /// <returns></returns>
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
        /// <param name="games"></param>
        /// <param name="take"></param>
        /// <returns></returns>
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
    }
}
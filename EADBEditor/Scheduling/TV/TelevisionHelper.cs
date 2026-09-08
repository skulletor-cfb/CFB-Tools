using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor.Scheduling.TV
{
    public static class TelevisionHelper
    {
        public static Dictionary<int, List<TelevisedGame>> GetAvailableGamesByWeek(
            this List<TelevisedGame> games, 
            Func<TelevisedGame,int> orderFunc = null,
            Func<TelevisedGame,bool> selector = null
            )
        {
            selector = selector ?? (tvg => !tvg.Selected);
            orderFunc = orderFunc ?? (tvg => tvg.Score);
            return games.GroupBy(g => g.Week)
                .ToDictionary(g => g.Key, g => g.Where(game => selector(game)).OrderBy(game => orderFunc(game)).ToList());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor.Scheduling.TV
{
    public class CBSNetwork : NetworkSchedule
    {
        public static readonly CBSNetwork Instance = new CBSNetwork();
        private CBSNetwork() : base("CBS")
        {
        }

        public override NetworkSchedule AssignGames()
        {
            foreach (var kvp in this.WeeklySchedule)
            {
                var queue = kvp.Value.ToQueue();

                while (queue.TryDequeueGame(out var game))
                {
                    if (game.IsBig10Game)
                    {
                        Primary.AssignGame(game, kvp.Key, 3, 30);
                    }
                    else
                    {
                        Primary.AssignGame(game, kvp.Key, 7, 30);
                    }
                }
            }

            return this;
        }

        public override void SelectGames(Dictionary<int, List<TelevisedGame>> televisedGames)
        {
            // every week get the third best Big 10 game
            var big10 = televisedGames[TableUtility.Big10Id].GetAvailableGamesByWeek();
            foreach (var kvp in big10)
            {
                if (kvp.Value.Count > 2)
                {
                    this.SelectedGames.Add(kvp.Value[2].Select());
                }
            }

            // every week get the best pac12 game
            var pac12 = televisedGames[TableUtility.Pac16Id].GetAvailableGamesByWeek();
            foreach (var kvp in pac12)
            {
                this.SelectedGames.Add(kvp.Value[0].Select());
            }
        }
    }
}
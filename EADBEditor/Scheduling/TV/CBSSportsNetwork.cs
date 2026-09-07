using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor.Scheduling.TV
{
    public class CBSSportsNetwork : NetworkSchedule
    {
        public static readonly CBSSportsNetwork Instance = new CBSSportsNetwork();
        private CBSSportsNetwork() : base("CBSSN")
        {
        }

        public override NetworkSchedule AssignGames()
        {
            bool noonNeeded = false;
            foreach (var kvp in this.WeeklySchedule)
            {
                var queue = kvp.Value.ToQueue();

                while (queue.TryDequeueGame(out var game))
                {
                    if (game.IsAirForce)
                    {
                        Primary.AssignGame(game, kvp.Key, 7, 30);
                        continue;
                    }

                    if (!noonNeeded)
                    {
                        Primary.AssignGame(game, kvp.Key, 3, 30);
                        noonNeeded = true;
                        continue;
                    }

                    Primary.AssignGame(game, kvp.Key, 12, 0);
                }
            }

            this.SelectedGames.ReturnInventory();
            return this;
        }

        public override void SelectGames(Dictionary<int, List<TelevisedGame>> televisedGames)
        {
            // get military military academy games left
            var games = televisedGames.Values.SelectMany(g => g).Where(g => g.IsMilitaryHomeGame && !g.Selected).ToArray();
            this.SelectedGames.AddRange(games.Select(g => g.Select()));
        }
    }
}
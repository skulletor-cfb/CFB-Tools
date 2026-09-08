using System.Collections.Generic;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

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
            foreach (var kvp in this.WeeklySchedule)
            {
                var late = new TimeSlot(7, 30, kvp.Key);
                var noon = new TimeSlot(12, 0, kvp.Key);
                var afternoon = new TimeSlot(3, 30, kvp.Key);

                // military games go first
                var queue = kvp.Value.Where(g => g.IsMilitaryHomeGame).ToQueue();

                while (queue.TryDequeueGame(out var game))
                {
                    if (game.IsAirForce)
                    {
                        Primary.AssignGame(game, late);
                        continue;
                    }

                    if( !Primary.ContainsKey(afternoon))
                    {
                        Primary.AssignGame(game, afternoon);
                        continue;
                    }

                    Primary.AssignGame(game, noon);
                }

                // cusa games
                queue = kvp.Value.Where(g => !g.IsMilitaryHomeGame).ToQueue();
                while (queue.TryDequeueGame(out var game))
                {
                    if (!Primary.ContainsKey(noon))
                    {
                        Primary.AssignGame(game, noon);
                        continue;
                    }

                    if (!Primary.ContainsKey(late))
                    {
                        Primary.AssignGame(game, late);
                        continue;
                    }

                    if (!Primary.ContainsKey(afternoon))
                    {
                        Primary.AssignGame(game, afternoon);
                        continue;
                    }
                }
            }

            this.SelectedGames.ReturnInventory();
            return this;
        }

        public override void SelectGames(Dictionary<int, List<TelevisedGame>> televisedGames)
        {
            // get military military academy games left
            var games = televisedGames.Values.SelectMany(g => g).Where(g => g.IsMilitaryHomeGame && !g.Selected).ToList();
            games.AddRange(televisedGames.Values.SelectMany(g => g).Where(g => g.IsCUSAGame && !g.Selected));
            this.SelectedGames.AddRange(games.Select(g => g.Select()));
        }
    }
}
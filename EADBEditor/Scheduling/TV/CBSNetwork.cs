using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor.Scheduling.TV
{
    public class CBSNetwork : NetworkSchedule
    {
        public static readonly CBSNetwork Instance = new CBSNetwork();
        private CBSNetwork() : base(ChannelName.CBS, StreamingProvider.ParamountPlus)
        {
        }

        public override bool PreassignGame(TelevisedGame game)
        {
            if (game.CheckMatchup(8, 57))
            {
                Primary.PreassignGame(game, new TimeSlot(3, 30, game.Week), true);
            }

            if (game.CheckMatchup(1, 8))
            {
                var timeslot = game.HomeTeam == 1 ? new TimeSlot(7, 30, game.Week) : new TimeSlot(3, 30, game.Week);
                Primary.PreassignGame(game, timeslot, true);
            }

            if (game.CheckMatchup(1, 57))
            {
                var timeslot = game.HomeTeam == 1 ? new TimeSlot(3, 30, game.Week) : new TimeSlot(12, 0, game.Week);
                Primary.PreassignGame(game, timeslot, true);
            }

            return base.PreassignGame(game);
        }

        public override NetworkSchedule AssignGames()
        {
            foreach (var kvp in this.WeeklySchedule)
            {
                var queue = kvp.Value.ToQueue();

                while (queue.TryDequeueGameForAssignment(out var game))
                {
                    if (game.IsBig10Game)
                    {
                        // CBS game is black friday at noon 
                        var slot = kvp.Key == 13 ? new TimeSlot(12, 0, week: kvp.Key, day: 4) : new TimeSlot(3, 30, week: kvp.Key);
                        Primary.AssignGame(game, slot);
                    }
                    else
                    {
                        var slot = new TimeSlot(7, 30, week: kvp.Key);
                        Primary.AssignGame(game, slot);
                    }
                }
            }

            // we might need to put games back into the pool
            this.SelectedGames.ReturnInventory();
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
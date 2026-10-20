using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor.Scheduling
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
                return !Primary.PreassignGame(game, new TimeSlot(3, 30, game.Week), true);
            }

            if (game.CheckMatchup(1, 8))
            {
                var timeslot = new TimeSlot(7, 30, game.Week);
                return !Primary.PreassignGame(game, timeslot, true);
            }

            if (game.CheckMatchup(1, 57))
            {
                var timeslot = new TimeSlot(12, 0, game.Week);
                return !Primary.PreassignGame(game, timeslot, true);
            }

            if (game.IsBig10Game)
            {
                if (game.Week == 13)
                {
                    return !Primary.PreassignGame(game, new TimeSlot(12, 0, game.Week, day: 4), true);
                }
                else
                {
                    return !Primary.PreassignGame(game, new TimeSlot(3, 30, game.Week, day: 5), true);
                }
            }

            return base.PreassignGame(game);
        }

        public override NetworkSchedule AssignGames()
        {
            // find the big 12 game
            var b12 = this.WeeklySchedule[13].Where(g => g.IsBig12Game).FirstOrDefault();
            Primary.AssignGame(b12, new TimeSlot(3, 30, b12.Week, day: 4));

            var mwc = this.SelectedGames.Games.Where(g => g.IsMWCGame).FirstOrDefault();
            Primary.AssignGame(mwc, new TimeSlot(7, 30, mwc.Week));

            // the rest should be pac 12 games on CBS
            var queue = this.SelectedGames.ToAssignmentQueue();
            while (queue.TryDequeueGameForAssignment(out var game))
            {
                Primary.AssignGame(game, new TimeSlot(7, 30, mwc.Week));
            }

            // we might need to put games back into the pool
            this.SelectedGames.ReturnInventory();
            return this;
        }
    }
}
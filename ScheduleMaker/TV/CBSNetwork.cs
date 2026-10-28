#nullable disable
using NCAA14Utility.Scheduling;

namespace ScheduleMaker.TV
{
    public class CBSNetwork : NetworkSchedule
    {
        public static CBSNetwork Instance { get; private set; }

        public static void Reset() => Instance = new CBSNetwork();

        private CBSNetwork() : base(ChannelName.CBS, StreamingProvider.ParamountPlus)
        {
        }

        public override bool PreassignGame(TelevisedGame game)
        {
            if (game.CheckMatchup(ScheduleBase.Army, ScheduleBase.Navy))
            {
                return !Primary.PreassignGame(game, new TimeSlot(3, 30, game.Week), true);
            }

            if (game.CheckMatchup(ScheduleBase.AF, ScheduleBase.Army))
            {
                var timeslot = new TimeSlot(7, 30, game.Week);
                return !Primary.PreassignGame(game, timeslot, true);
            }

            if (game.CheckMatchup(ScheduleBase.AF, ScheduleBase.Navy))
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
            var b12 = this.WeeklySchedule.Week(13).Where(g => g.IsBig12Game).FirstOrDefault();
            if (b12 != null)
            {
                Primary.AssignGame(b12, new TimeSlot(3, 30, b12.Week, day: 4));
            }

            var mwc = this.SelectedGames.Games.Where(g => g.IsMWCGame).FirstOrDefault();
            if (mwc != null)
            {
                Primary.AssignGame(mwc, new TimeSlot(7, 30, mwc.Week));

                // the rest should be pac 12 games on CBS
                var queue = this.SelectedGames.ToAssignmentQueue();
                while (queue.TryDequeueGameForAssignment(out var game))
                {
                    Primary.AssignGame(game, new TimeSlot(7, 30, mwc.Week));
                }
            }

            // we might need to put games back into the pool
            this.SelectedGames.ReturnInventory();
            return this;
        }
    }
}

using System;
using System.Linq;

namespace EA_DB_Editor.Scheduling
{
    public class CBSSportsNetwork : NetworkSchedule
    {
        public static readonly CBSSportsNetwork Instance = new CBSSportsNetwork();
        private CBSSportsNetwork() : base(ChannelName.CBSSportsNetwork, StreamingProvider.None)
        {
        }

        public override bool PreassignGame(TelevisedGame game)
        {
            // military academy games played on cbs, nd-navy on espn
            if (game.CheckMatchup(8, 57) || game.CheckMatchup(1, 8) || game.CheckMatchup(1, 57) || game.CheckMatchup(57, 68))
            {
                return false;
            }

            // the rest of army/navy is played on CBSSN
            if (game.IsArmyGame)
            {
                var timeslot = new TimeSlot(12, 0, game.Week);
                return !Primary.PreassignGame(game, timeslot, true);
            }
            else if (game.IsNavyGame)
            {
                var timeslot = new TimeSlot(3, 30, game.Week);
                return !Primary.PreassignGame(game, timeslot, true);
            }

            return base.PreassignGame(game);
        }

        private void AssignMACtion()
        {
            TelevisedGame game = null;
            var firstWeekOfNovember = TelevisionScheduler.LastWeekOfOctober() + 1;
            for (int i = firstWeekOfNovember; i <= 12; i++)
            {
                var queue = this.WeeklySchedule[i].Where(g => !g.Assigned && g.IsMACGame).OrderBy(g => g.Score).ToQueue();

                // maction
                if (queue.TryDequeueGameForAssignment(out game))
                {
                    Primary.AssignGame(game, new TimeSlot(8, 0, i, day: 1));
                }

                if (queue.TryDequeueGameForAssignment(out game))
                {
                    Primary.AssignGame(game, new TimeSlot(8, 0, i, day: 2));
                }
            }

            // week 13 goes tuesday, friday, saturday
            var lastWeek = this.WeeklySchedule[13].Where(g => !g.Assigned && g.IsMACGame).OrderBy(g => g.Score).ToQueue();
            // week 13 mac
            if (lastWeek.TryDequeueGameForAssignment(out game))
            {
                Primary.AssignGame(game, new TimeSlot(12, 0, 13, day: 5));
            }

            if (lastWeek.TryDequeueGameForAssignment(out game))
            {
                Primary.AssignGame(game, new TimeSlot(12, 30, 13, day: 4));
            }

            if (lastWeek.TryDequeueGameForAssignment(out game))
            {
                Primary.AssignGame(game, new TimeSlot(7, 30, 13, day: 1));
            }
        }

        /// <summary>
        /// assigm the MWC games at 7 and 1030
        /// </summary>
        private void AssignMWC()
        {
            Func<int, TimeSlot> evening = w => new TimeSlot(7, 0, w);
            Func<int, TimeSlot> late = w => new TimeSlot(10, 30, w);
            foreach (var kvp in this.WeeklySchedule)
            {
                Func<int, TimeSlot> other = late;

                // should not be more than 2
                var queue = kvp.Value.Where(g => g.IsMWCGame).ToQueue();

                if (queue.TryDequeueGameForAssignment(out var game))
                {
                    if (game.IsHawaiiGame)
                    {
                        Primary.AssignGame(game, game.CalculateHawaiiTimeSlot());
                        other = evening;
                    }
                    else
                    {
                        Primary.AssignGame(game, evening(game.Week));
                    }
                }

                if (queue.TryDequeueGameForAssignment(out game))
                {
                    if (game.IsHawaiiGame)
                    {
                        Primary.AssignGame(game, game.CalculateHawaiiTimeSlot());
                    }
                    else
                    {
                        Primary.AssignGame(game, other(game.Week));
                    }
                }
            }
        }

        public override NetworkSchedule AssignGames()
        {
            AssignMACtion();
            AssignMWC();

            foreach (var kvp in this.WeeklySchedule)
            {
                var noon = new TimeSlot(12, 0, week: kvp.Key);
                var afternoon = new TimeSlot(3, 30, week: kvp.Key);

                // cusa/mac games
                var queue = kvp.Value.ToQueue();
                while (queue.TryDequeueGameForAssignment(out var game))
                {
                    if (Primary.AssignGame(game, noon))
                    {
                        continue;
                    }

                    if (Primary.AssignGame(game, afternoon))
                    {
                        continue;
                    }
                }
            }

            this.SelectedGames.ReturnInventory();
            return this;
        }
    }
}
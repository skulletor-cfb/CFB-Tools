using System;
using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor.Scheduling
{
    public class CWNetwork : NetworkSchedule
    {
        private class WeekSchedule
        {
            public TimeSlot Early { get; set; }

            public TimeSlot Afternoon { get; set; }
            public TimeSlot Evening { get; set; }
            public TimeSlot Late { get; set; }
            public TimeSlot FridayNight { get; set; }
        }

        public static readonly CWNetwork Instance = new CWNetwork();
        private CWNetwork() : base(ChannelName.CW, StreamingProvider.None)
        {
        }

        private void AssignACC()
        {
            foreach (var kvp in this.WeeklySchedule)
            {
                var game = kvp.Value.Where(g => g.IsAccGame).FirstOrDefault();
                var timeslot = game.HomeRank <= 25 ? new TimeSlot(3, 30, kvp.Key) : new TimeSlot(12, 0, kvp.Key);
                Primary.AssignGame(game, timeslot);
            }
        }

        /// <summary>
        /// assigm the MWC games at 7 and 1030
        /// </summary>
        private void AssignMWC()
        {
            // the best week 13 game goes to friday 430pm, can't be hawaii though
            var game = this.WeeklySchedule[13].Where(g => g.IsMWCGame && !g.IsHawaiiGame).FirstOrDefault();
            Primary.AssignGame(game, 13, 4, 30, day: 4);

            foreach (var kvp in this.WeeklySchedule)
            {
                // get our games
                var games = kvp.Value.Where(g => g.IsMWCGame).ToList();

                var friday = new TimeSlot(9, 0, kvp.Key, day: 4);
                var early = new TimeSlot(3, 30, kvp.Key);
                var evening = new TimeSlot(7, 0, kvp.Key);
                var late = AssignHawaiiGames(games, kvp.Key);

                var stack = new Stack<TimeSlot>(new[] { late, friday, early, evening }.Where(ts => ts != null).Where(ts => Primary.IsTimeslotAvailable(ts)).ToArray().Shuffle());
                var queue = games.ToQueue();

                while (queue.TryDequeueGameForAssignment(out game))
                {
                    if (stack.TryPop(out var timeslot))
                    {
                        Primary.AssignGame(game, timeslot);
                    }
                }
            }
        }

        private TimeSlot AssignHawaiiGames(List<TelevisedGame> games, int week)
        {
            // any hawaii games
            var hawaiiGame = games.Where(g => g.IsHawaiiGame).FirstOrDefault();
            if (hawaiiGame != null && Primary.AssignGame(hawaiiGame, hawaiiGame.CalculateHawaiiTimeSlot()))
            {
                return null;
            }

            return new TimeSlot(10, 30, week);
        }

        /// <summary>
        /// mwc plays 
        /// hawaii game: 1230, 400, 730, 1130
        /// otherwise: 1200, 330, 7, 1030
        /// </summary>
        /// <returns></returns>
        public override NetworkSchedule AssignGames()
        {
            AssignACC();
            AssignMWC();
            this.SelectedGames.ReturnInventory();
            return this;
        }
    }
}
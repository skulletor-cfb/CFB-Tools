
using System;
using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor.Scheduling
{
    public class MountainWestSportsNetwork : NetworkSchedule
    {
        public static readonly MountainWestSportsNetwork Instance = new MountainWestSportsNetwork();
        private MountainWestSportsNetwork() : base(ChannelName.TheMW, StreamingProvider.MWPlus)
        {
        }

        public override NetworkSchedule AssignGames()
        {
            return this;
        }

        public override void AssignStreaming()
        {
            //  hawaii only plays on saturday
            var hawaii = SelectedGames.Games.Where(g => g.IsHawaiiGame);
            foreach (var game in hawaii)
            {
                Streaming.AssignGame(game, game.CalculateHawaiiTimeSlot());
            }

            var mst = new Stack<Func<int, TimeSlot>>(
                new Func<int, TimeSlot>[]
                {
                    w=>new TimeSlot(1,0,w) ,
                    w=>new TimeSlot(3,0,w) ,
                    w=>new TimeSlot(9,30,w) ,
                    w=>new TimeSlot(6,0,w) ,
                }.Shuffle());

            var pst = new Stack<Func<int, TimeSlot>>(
                new Func<int, TimeSlot>[]
                {
                    w=>new TimeSlot(11,0,w) ,
                    w=>new TimeSlot(7,0,w) ,
                    w=>new TimeSlot(3,0,w) ,
                    w=>new TimeSlot(6,30,w) ,
                }.Shuffle());


            var queue = SelectedGames.ToAssignmentQueue();
            while (queue.TryDequeueGameForAssignment(out var game))
            {
                Streaming.AssignGame(game, game.CalculateTimeSlot());
            }
        }
    }

    public static class TimeZoneHelper
    {
        private static readonly TimeZoneInfo HonoluluZone =
            TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time");

        private static readonly TimeZoneInfo NewYorkZone =
            TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        private static HashSet<int> MountainTime = new HashSet<int>
        {
            23,
            104,
            115,
            23,
            1,
            60
        };

        /// <summary>
        /// Converts a DateTime that represents local Honolulu time
        /// into the equivalent local New York City time.
        /// </summary>
        public static DateTime HonoluluToNewYork(DateTime honoluluTime)
        {
            // Honolulu doesn't observe DST, so this always shifts a fixed 5 or 6 hours
            // depending on whether NYC is currently in EDT or EST.
            DateTime unspecified = DateTime.SpecifyKind(honoluluTime, DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTime(unspecified, HonoluluZone, NewYorkZone);
        }

        public static TimeSlot CalculateHawaiiTimeSlot(this TelevisedGame game)
        {
            // hawaii plays its games at 6pm local time
            var gameDay = TelevisionScheduler.CurrentSeason.Weeks[game.Week];
            var hst = new DateTime(gameDay.Year, gameDay.Month, gameDay.Day, 18, 0, 0);
            var est = HonoluluToNewYork(hst);

            // don't go past the minute in est
            if (est.Hour == 0)
            {
                est = new DateTime(hst.Year, hst.Month, hst.Day, 23, 59, 0);
            }

            var gtod = new GameTimeOfDay(est);
            return new TimeSlot(gtod, game.Week, 5);
        }

        public static TimeSlot CalculateTimeSlot(this TelevisedGame game)
        {
            var mstTimes = new Func<int, TimeSlot>[]
                {
                    w=>new TimeSlot(1,0,w) ,
                    w=>new TimeSlot(3,0,w) ,
                    w=>new TimeSlot(9,30,w) ,
                    w=>new TimeSlot(6,0,w) ,
                }.Shuffle();

            var pstTimes = new Func<int, TimeSlot>[]
                {
                    w=>new TimeSlot(11,0,w) ,
                    w=>new TimeSlot(7,0,w) ,
                    w=>new TimeSlot(3,0,w) ,
                    w=>new TimeSlot(6,30,w) ,
                }.Shuffle();

            var mst = new Stack<Func<int, TimeSlot>>(mstTimes.Concat(mstTimes.CreateAndShuffle()));
            var pst = new Stack<Func<int, TimeSlot>>(pstTimes.Concat(pstTimes.CreateAndShuffle()));

            if (MountainTime.Contains(game.HomeTeam))
            {
                return mst.Pop()(game.Week);
            }

            return pst.Pop()(game.Week);
        }
    }
}
using System;
using System.Collections.Generic;

namespace NCAA14Utility.Scheduling
{
    public class CUSASchedule : ScheduleBase
    {
        public CUSASchedule(int year) : base(year, 9, ScheduleHelpers.CUSA, nameof(ScheduleHelpers.CUSA))
        {
        }

        protected override Func<Dictionary<int, int[]>>[] Creators => new Func<Dictionary<int, int[]>>[] {
            CreateA, CreateA,
        };

        protected override int InitialYear => 2576;

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Army.Create(MTSU, LT, UTEP, UTSA),
                Navy.Create(Army, WKU, FAU, NT),
                MTSU.Create(Navy, LT, UTEP, NT),
                WKU.Create(Army, MTSU, LT, UTSA),
                LT.Create(Navy, FAU, UTEP, UTSA),
                FAU.Create(Army, MTSU, WKU, NT),
                UTEP.Create(Navy, WKU, FAU, UTSA),
                UTSA.Create(Navy, MTSU, FAU, NT),
                NT.Create(Army, WKU, LT, UTEP),
            }.Create();
        }
    }
}

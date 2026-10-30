using System;
using System.Collections.Generic;
using System.Linq;

namespace NCAA14Utility.Scheduling
{
    public class CUSASchedule : ScheduleBase
    {
        public CUSASchedule(int year) : base(year, 10, ScheduleHelpers.CUSA, nameof(ScheduleHelpers.CUSA))
        {
        }

        protected override Func<Dictionary<int, int[]>>[] Creators => new Func<Dictionary<int, int[]>>[] {
            CreateA, CreateA,
        };

        protected override int InitialYear => 2589;

        // EADBEditor's CUSALocks; Army-Navy is a shared lock
        protected override Func<int, int, int?>[] LockChecks => new Func<int, int, int?>[]
        {
            (h, a) => MatchTeams(13, h, a, MTSU, WKU),
        };

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                MTSU.Create(Delaware, KennesawSt, FIU, LT, SHSU),
                WKU.Create(MTSU, Liberty, JaxSt, MissouriSt, LT),
                Delaware.Create(WKU, KennesawSt, FIU, SHSU),
                Liberty.Create(MTSU, Delaware, JaxSt, MissouriSt, LT),
                KennesawSt.Create(WKU, Liberty, FIU, SHSU),
                JaxSt.Create(MTSU, Delaware, KennesawSt, MissouriSt, LT ),
                FIU.Create(WKU, Liberty, JaxSt, SHSU),
                MissouriSt.Create(MTSU, Delaware, KennesawSt, FIU, LT),
                LT.Create(Delaware, KennesawSt, FIU, SHSU),
                SHSU.Create(WKU, Liberty, JaxSt, MissouriSt),
            }.Create();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace NCAA14Utility.Scheduling
{
    public class MWCSchedule : ScheduleBase
    {
        public MWCSchedule(int year) : base(year, 16, ScheduleHelpers.MWC, nameof(ScheduleHelpers.MWC))
        {
        }

        protected override Func<Dictionary<int, int[]>>[] Creators => new Func<Dictionary<int, int[]>>[] {
            CreateA, CreateA,
            CreateB, CreateB,
        };

        protected override int InitialYear => 2589;

        // EADBEditor's MWCLocks
        protected override Func<int, int, int?>[] LockChecks => new Func<int, int, int?>[]
        {
        };

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            Wyoming.Create(AF, Hawaii, UNLV, UtahSt, SDSU),
            CSU.Create(Wyoming, UNM, NMSU, NDSU),
            AF.Create(CSU, FresnoSt, UTEP, NDSU),
            Hawaii.Create(AF, UNLV, UtahSt, SacState, UTEP),
            BoiseSt.Create(CSU, Hawaii, FresnoSt, SDSU, SacState),
            Nevada.Create(CSU, BoiseSt, UTEP, NDSU),
            UNLV.Create(AF, BoiseSt, Nevada, UNM, SacState),
            UNM.Create(Wyoming, AF, NMSU, NDSU),
            UtahSt.Create(BoiseSt, Nevada, SJSU, NMSU),
            FresnoSt.Create(Nevada, UNM, UtahSt, SDSU, NDSU),
            SJSU.Create(Wyoming, Hawaii, UNLV, FresnoSt, NMSU),
            SDSU.Create(CSU, Nevada, UNM, SJSU, SacState),
            SacState.Create(CSU, AF, Nevada, SJSU),
            UTEP.Create(UNM, UtahSt, FresnoSt, SDSU),
            NMSU.Create(Hawaii, BoiseSt, UNLV, SacState, UTEP),
            NDSU.Create(Wyoming, UtahSt, SJSU, UTEP),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            Wyoming.Create(BoiseSt, UtahSt, FresnoSt, SacState, UTEP),
            CSU.Create(Wyoming, UNLV, SJSU, NDSU),
            AF.Create(CSU, Nevada, UtahSt, UTEP),
            Hawaii.Create(CSU, AF, UNLV, UNM, NDSU),
            BoiseSt.Create(AF, Hawaii, UNM, FresnoSt, NDSU),
            Nevada.Create(Wyoming, Hawaii, BoiseSt, NMSU),
            UNLV.Create(Nevada, UNM, UtahSt, UTEP, NDSU),
            UNM.Create(Nevada, UtahSt, SJSU, NMSU),
            UtahSt.Create(CSU, SDSU, SacState, NMSU),
            FresnoSt.Create(CSU, Hawaii, UNLV, SDSU, NMSU),
            SJSU.Create(AF, BoiseSt, Nevada, FresnoSt, UTEP),
            SDSU.Create(AF, Hawaii, UNLV, SJSU, SacState),
            SacState.Create(Nevada, UNM, FresnoSt, SJSU),
            UTEP.Create(CSU, BoiseSt, UNM, SacState),
            NMSU.Create(Wyoming, AF, SDSU, UTEP, NDSU),
            NDSU.Create(Wyoming, UtahSt, SDSU, SacState),
            }.Create();
        }
    }
}

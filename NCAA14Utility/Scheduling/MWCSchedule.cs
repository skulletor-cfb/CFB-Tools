using System;
using System.Collections.Generic;
using System.Linq;

namespace NCAA14Utility.Scheduling
{
    public class MWCSchedule : ScheduleBase
    {
        public MWCSchedule(int year) : base(year, 12, ScheduleHelpers.MWC, nameof(ScheduleHelpers.MWC))
        {
        }

        protected override Func<Dictionary<int, int[]>>[] Creators => new Func<Dictionary<int, int[]>>[] {
            CreateA, CreateB,
            CreateC, CreateA,
            CreateB, CreateC,
        };

        protected override int InitialYear => 2545;

        // EADBEditor's MWCLocks
        protected override Func<int, int, int?>[] LockChecks => new Func<int, int, int?>[]
        {
        };

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wyoming.Create(AF, Nevada, UtahSt, SJSU),
                CSU.Create(Wyoming, Hawaii, UNM, SDSU),
                AF.Create(CSU, UNLV, UtahSt, FresnoSt),
                Hawaii.Create(Wyoming, AF, Nevada, SDSU),
                BSU.Create(CSU, Hawaii, UNLV, FresnoSt),
                Nevada.Create(AF, BSU, UNM, SJSU),
                UNLV.Create(CSU, Nevada, UtahSt, SDSU),
                UNM.Create(Wyoming, BSU, UNLV, FresnoSt),
                UtahSt.Create(Hawaii, BSU, UNM, SJSU),
                FresnoSt.Create(CSU, Nevada, UtahSt, SDSU),
                SJSU.Create(AF, Hawaii, UNLV, FresnoSt),
                SDSU.Create(Wyoming, BSU, UNM, SJSU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wyoming.Create(AF, Nevada, UtahSt, FresnoSt),
                CSU.Create(Wyoming, BSU, UtahSt, FresnoSt),
                AF.Create(CSU, UNLV, UNM, SJSU),
                Hawaii.Create(AF, UNLV, UNM, SDSU),
                BSU.Create(Wyoming, AF, Hawaii, FresnoSt),
                Nevada.Create(CSU,  Hawaii, BSU, SDSU),
                UNLV.Create(Wyoming, Nevada, UtahSt, SDSU),
                UNM.Create(CSU, BSU, UNLV, SJSU),
                UtahSt.Create(Hawaii, Nevada, UNM, SJSU),
                FresnoSt.Create(Hawaii, UNLV, UNM, SDSU),
                SJSU.Create(CSU, BSU,  Nevada, FresnoSt),
                SDSU.Create(Wyoming, AF, UtahSt, SJSU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wyoming.Create(BSU, UNLV, UtahSt, SJSU),
                CSU.Create(Wyoming, Hawaii, Nevada,SDSU),
                AF.Create(CSU, UNM, UtahSt, FresnoSt),
                Hawaii.Create(Wyoming, AF, UNLV, SJSU),
                BSU.Create(AF, Hawaii, UtahSt, SDSU),
                Nevada.Create(AF, BSU, UNM, FresnoSt),
                UNLV.Create(CSU, BSU, Nevada, SJSU),
                UNM.Create(Wyoming, Hawaii, UNLV, SDSU),
                UtahSt.Create(CSU, Nevada, UNM, FresnoSt),
                FresnoSt.Create(Wyoming, Hawaii, UNLV, SDSU),
                SJSU.Create(CSU, BSU, UNM, FresnoSt),
                SDSU.Create(AF, Nevada, UtahSt, SJSU),
            }.Create();
        }
    }
}

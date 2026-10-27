using System;
using System.Collections.Generic;
using System.Linq;

namespace NCAA14Utility.Scheduling
{
    public class SECSchedule : ScheduleBase
    {
        public SECSchedule(int year) : base(year, 14, ScheduleHelpers.SEC, nameof(ScheduleHelpers.SEC))
        {
        }

        protected override Func<Dictionary<int, int[]>>[] Creators => new Func<Dictionary<int, int[]>>[] {
            CreateA, CreateA,
            CreateB, CreateB,
        };

        protected override int InitialYear => 2548;

        // EADBEditor's SecLocks. UGA-GT, FSU-UF, Clemson-South Carolina and Louisville-Kentucky are shared locks.
        protected override Func<int, int, int?>[] LockChecks => new Func<int, int, int?>[]
        {
            (h, a) => MatchTeams(13, h, a, Alabama, Auburn),
            (h, a) => MatchTeams(13, h, a, MissSt, OleMiss),
            (h, a) => MatchTeams(13, h, a, Arkansas, LSU),
            (h, a) => MatchTeams(13, h, a, Missouri, TAMU),
            (h, a) => MatchTeams(13, h, a, Vandy, Tennessee),
            (h, a) => MatchTeams(this.LastWeekOfOctober, h, a, UF, UGA),
            (h, a) => MatchTeams(this.LaborDayWeek + 1, h, a, UGA, SCAR),
            (h, a) => MatchTeams(this.WeekOf(this.Calendar.ThirdSaturdayInOctober), h, a, Alabama, Tennessee),
            (h, a) => MatchTeams(12, h, a, UK, Tennessee),
            (h, a) => MatchTeams(this.WeekOf(this.Calendar.ThirdSaturdayInSeptember), h, a, UF, Tennessee),
            (h, a) => MatchTeams(this.WeekOf(this.Calendar.LastSaturdayInSeptember), h, a, Arkansas, TAMU),
        };

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Alabama.Create(UF, LSU, Tennessee, TAMU),
                Arkansas.Create(MissSt, Missouri, Tennessee, TAMU),
                Auburn.Create(Alabama, Arkansas,OleMiss, SCAR),
                UF.Create(Auburn, UGA, OleMiss, Vandy),
                UGA.Create(Arkansas, Auburn,MissSt, SCAR),
                UK.Create(Alabama, UF, Missouri, Tennessee),
                LSU.Create(Arkansas, UGA, SCAR, Vandy),
                MissSt.Create(Alabama, UK, LSU, Vandy),
                Missouri.Create(Alabama, UGA, SCAR, TAMU),
                OleMiss.Create(LSU, MissSt, Missouri, Tennessee),
                SCAR.Create(Arkansas, UF, UK, OleMiss),
                Tennessee.Create(Auburn,UGA, TAMU, Vandy),
                TAMU.Create(UF, UK, LSU, MissSt),
                Vandy.Create(Auburn, UK, Missouri, OleMiss),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Alabama.Create(Arkansas, UGA, SCAR, Tennessee),
                Arkansas.Create(UF, Missouri, TAMU, Vandy),
                Auburn.Create(Alabama, MissSt, Missouri, TAMU),
                UF.Create(Auburn, UGA, Missouri, Tennessee),
                UGA.Create(Auburn, UK, OleMiss, SCAR),
                UK.Create(Arkansas, Auburn, OleMiss, Tennessee),
                LSU.Create(Arkansas, Auburn, UF, UK),
                MissSt.Create(Alabama, UF, UK, Missouri),
                Missouri.Create(LSU, SCAR, Tennessee, TAMU),
                OleMiss.Create(Alabama, Arkansas, LSU, MissSt),
                SCAR.Create(UF, MissSt, TAMU, Vandy),
                Tennessee.Create(LSU, MissSt, SCAR, Vandy),
                TAMU.Create(UGA, LSU, OleMiss, Vandy),
                Vandy.Create(Alabama, UGA, UK, OleMiss),
            }.Create();
        }
    }
}

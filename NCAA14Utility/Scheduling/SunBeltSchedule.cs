using System;
using System.Collections.Generic;
using System.Linq;

namespace NCAA14Utility.Scheduling
{
    public class SunBeltSchedule : ScheduleBase
    {
        public SunBeltSchedule(int year) : base(year, 14, ScheduleHelpers.SBC, nameof(ScheduleHelpers.SBC))
        {
        }

        protected override Func<Dictionary<int, int[]>>[] Creators => new Func<Dictionary<int, int[]>>[] {
            CreateA, CreateB,
            CreateC, CreateD,
            CreateE, CreateF,
            CreateG, CreateA,
            CreateB, CreateC,
            CreateD, CreateE,
            CreateF, CreateG,
        };

        protected override int InitialYear => 2546;

        // EADBEditor's SunBeltLocks
        protected override Func<int, int, int?>[] LockChecks => new Func<int, int, int?>[]
        {
            (h, a) => MatchTeams(13, h, a, AppSt, GASO),
            (h, a) => MatchTeams(13, h, a, Troy, USA),
            (h, a) => MatchTeams(13, h, a, JMU, ODU),
            (h, a) => MatchTeams(13, h, a, ULM, ULL),
            (h, a) => MatchTeams(13, h, a, ArkansasSt, NT),
            (h, a) => MatchTeams(7, h, a, GASO, GSU),
            (h, a) => MatchTeams(8, h, a, AppSt, Marshall),
        };

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkansasSt.Create(USA, ULM, USM, Marshall),
                USA.Create(USM, TexasSt, ULL, JMU),
                ULM.Create(USA, USM, ULL, AppSt),
                USM.Create(TexasSt, Troy, ULL, GSU),
                TexasSt.Create(ArkansasSt, ULM, Troy, ODU),
                Troy.Create(ArkansasSt, USA, ULM, CCU),
                ULL.Create(ArkansasSt, TexasSt, Troy, GASO),

                Marshall.Create(ULL, GSU, ODU, CCU),
                JMU.Create(ArkansasSt, Marshall, GSU, CCU),
                AppSt.Create(USA, Marshall, JMU, CCU),
                GSU.Create(ULM, AppSt, ODU, GASO),
                ODU.Create(USM, JMU, AppSt, GASO),
                CCU.Create(TexasSt, GSU, ODU, GASO),
                GASO.Create(Troy, Marshall, JMU, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkansasSt.Create(USA, ULM, USM, AppSt),
                USA.Create(USM, TexasSt, ULL, GSU),
                ULM.Create(USA, USM, ULL, ODU),
                USM.Create(TexasSt, Troy, ULL, CCU),
                TexasSt.Create(ArkansasSt, ULM, Troy, GASO),
                Troy.Create(ArkansasSt, USA, ULM, Marshall),
                ULL.Create(ArkansasSt, TexasSt, Troy, JMU),

                Marshall.Create(TexasSt, GSU, ODU, CCU),
                JMU.Create(Troy, Marshall, GSU, CCU),
                AppSt.Create(ULL, Marshall, JMU, CCU),
                GSU.Create(ArkansasSt, AppSt, ODU, GASO),
                ODU.Create(USA, JMU, AppSt, GASO),
                CCU.Create(ULM, GSU, ODU, GASO),
                GASO.Create(USM, Marshall, JMU, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkansasSt.Create(USA, ULM, USM, ODU),
                USA.Create(USM, TexasSt, ULL, CCU),
                ULM.Create(USA, USM, ULL, GASO),
                USM.Create(TexasSt, Troy, ULL, Marshall),
                TexasSt.Create(ArkansasSt, ULM, Troy, JMU),
                Troy.Create(ArkansasSt, USA, ULM, AppSt),
                ULL.Create(ArkansasSt, TexasSt, Troy, GSU),

                Marshall.Create(ULM, GSU, ODU, CCU),
                JMU.Create(USM, Marshall, GSU, CCU),
                AppSt.Create(TexasSt, Marshall, JMU, CCU),
                GSU.Create(Troy, AppSt, ODU, GASO),
                ODU.Create(ULL, JMU, AppSt, GASO),
                CCU.Create(ArkansasSt, GSU, ODU, GASO),
                GASO.Create(USA, Marshall, JMU, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkansasSt.Create(USA, ULM, USM, GASO),
                USA.Create(USM, TexasSt, ULL, Marshall),
                ULM.Create(USA, USM, ULL, JMU),
                USM.Create(TexasSt, Troy, ULL, AppSt),
                TexasSt.Create(ArkansasSt, ULM, Troy, GSU),
                Troy.Create(ArkansasSt, USA, ULM, ODU),
                ULL.Create(ArkansasSt, TexasSt, Troy, CCU),

                Marshall.Create(ArkansasSt, GSU, ODU, CCU),
                JMU.Create(USA, Marshall, GSU, CCU),
                AppSt.Create(ULM, Marshall, JMU, CCU),
                GSU.Create(USM, AppSt, ODU, GASO),
                ODU.Create(TexasSt, JMU, AppSt, GASO),
                CCU.Create(Troy, GSU, ODU, GASO),
                GASO.Create(ULL, Marshall, JMU, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkansasSt.Create(USA, ULM, USM, JMU),
                USA.Create(USM, TexasSt, ULL, AppSt),
                ULM.Create(USA, USM, ULL, GSU),
                USM.Create(TexasSt, Troy, ULL, ODU),
                TexasSt.Create(ArkansasSt, ULM, Troy, CCU),
                Troy.Create(ArkansasSt, USA, ULM, GASO),
                ULL.Create(ArkansasSt, TexasSt, Troy, Marshall),

                Marshall.Create(Troy, GSU, ODU, CCU),
                JMU.Create(ULL, Marshall, GSU, CCU),
                AppSt.Create(ArkansasSt, Marshall, JMU, CCU),
                GSU.Create(USA, AppSt, ODU, GASO),
                ODU.Create(ULM, JMU, AppSt, GASO),
                CCU.Create(USM, GSU, ODU, GASO),
                GASO.Create(TexasSt, Marshall, JMU, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateF()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkansasSt.Create(USA, ULM, USM, GSU),
                USA.Create(USM, TexasSt, ULL, ODU),
                ULM.Create(USA, USM, ULL, CCU),
                USM.Create(TexasSt, Troy, ULL, GASO),
                TexasSt.Create(ArkansasSt, ULM, Troy, Marshall),
                Troy.Create(ArkansasSt, USA, ULM, JMU),
                ULL.Create(ArkansasSt, TexasSt, Troy, AppSt),

                Marshall.Create(USM, GSU, ODU, CCU),
                JMU.Create(TexasSt, Marshall, GSU, CCU),
                AppSt.Create(Troy, Marshall, JMU, CCU),
                GSU.Create(ULL, AppSt, ODU, GASO),
                ODU.Create(ArkansasSt, JMU, AppSt, GASO),
                CCU.Create(USA, GSU, ODU, GASO),
                GASO.Create(ULM, Marshall, JMU, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateG()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkansasSt.Create(USA, ULM, USM, CCU),
                USA.Create(USM, TexasSt, ULL, GASO),
                ULM.Create(USA, USM, ULL, Marshall),
                USM.Create(TexasSt, Troy, ULL, JMU),
                TexasSt.Create(ArkansasSt, ULM, Troy, AppSt),
                Troy.Create(ArkansasSt, USA, ULM, GSU),
                ULL.Create(ArkansasSt, TexasSt, Troy, ODU),

                Marshall.Create(USA, GSU, ODU, CCU),
                JMU.Create(ULM, Marshall, GSU, CCU),
                AppSt.Create(USM, Marshall, JMU, CCU),
                GSU.Create(TexasSt, AppSt, ODU, GASO),
                ODU.Create(Troy, JMU, AppSt, GASO),
                CCU.Create(ULL, GSU, ODU, GASO),
                GASO.Create(ArkansasSt, Marshall, JMU, AppSt),
            }.Create();
        }
    }
}

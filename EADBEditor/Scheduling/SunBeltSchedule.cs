using System;
using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor
{
    public class SunBeltSchedule
    {
        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { 
            CreateA, CreateA, CreateB, CreateB, };
        public static Dictionary<int, HashSet<int>> SunbeltConferenceSchedule = null;
        public static Dictionary<int, int[]> ScenarioForSeason = null;
        public static HashSet<int> East = new HashSet<int>() { LT, MTSU, USM, UAB, Troy, USA };
        public static HashSet<int> West = new HashSet<int>() { TexSt, ArkSt, NT, UTSA, ULM, ULL};

        public static bool CrossDivision(int a, int b)
        {
            var sameDivision = (East.Contains(a) && East.Contains(b)) || (West.Contains(a) && West.Contains(b));
            return !sameDivision;
        }

        public static void Init()
        {
            if (!initRun)
            {
                ScenarioForSeason = CreateScenarioForSeason();
                initRun = true;
            }
        }

        public static void ProcessSunbeltSchedule(Dictionary<int, PreseasonScheduledGame[]> schedule)
        {
            schedule.ProcessSchedule(ScenarioForSeason, SunbeltConferenceSchedule, RecruitingFixup.SBCId, RecruitingFixup.SBC);
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var idx = (Form1.DynastyYear - 2446) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(12, RecruitingFixup.SBCId, "SunBelt");
            SunbeltConferenceSchedule = result.BuildHashSet();
            return result;
        }

        const int Troy = 143;
        const int USA = 235;
        const int USM = 85;
        const int UAB = 98;
        const int LT = 43;
        const int MTSU = 53;

        const int UTSA = 232;
        const int TexSt = 218;
        const int ArkSt = 7;
        const int NT = 64;
        const int ULM = 65;
        const int ULL = 86;

#if true // sun belt is a southern eastern conference
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                NT.Create(ULL, ArkSt, UTSA, USM),
                ULL.Create(TexSt, ArkSt, Troy, LT),
                TexSt.Create(NT, ArkSt, ULM, LT),
                ArkSt.Create(UTSA, ULM, USA, MTSU),
                UTSA.Create(ULL, TexSt, ULM, UAB),
                ULM.Create(NT, ULL, USA, MTSU),

                USA.Create(UTSA, MTSU, UAB, USM),
                MTSU.Create(UTSA, USM, Troy, LT),
                UAB.Create(ArkSt, ULM, MTSU, Troy),
                USM.Create(ULL, TexSt, UAB, Troy),
                Troy.Create(NT, TexSt, USA, LT),
                LT.Create(NT, USA, UAB, USM),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }

#elif false //uab and usm make 16, 8 conference games
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UAB.Create(LT, ULL, TexSt, ArkSt),
                LT.Create(NT, ULM, USM, Marshall),
                NT.Create(UAB, ULL, ArkSt, USM),
                ULL.Create(LT, TexSt, ArkSt, ODU),
                TexSt.Create(LT, NT, ULM, USM),
                ULM.Create(UAB, NT, ULL, GaSo),
                ArkSt.Create(LT, TexSt, ULM, USM),
                USM.Create(UAB, ULL, ULM, Coastal),

                Troy.Create(Marshall, GSU, USA, UAB),
                Marshall.Create(GSU, ODU, USA, Coastal),
                GSU.Create(USA, GaSo, AppSt, NT),
                ODU.Create(Troy, GSU, AppSt, Coastal),
                USA.Create(ODU, GaSo, AppSt, TexSt),
                GaSo.Create(Troy, Marshall, ODU, AppSt),
                AppSt.Create(Troy, Marshall, Coastal, ArkSt),
                Coastal.Create(Troy, GSU, USA, GaSo),

            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UAB.Create(LT, ULL, TexSt, ArkSt),
                LT.Create(NT, ULM, USM, ODU),
                NT.Create(UAB, ULL, ArkSt, USM),
                ULL.Create(LT, TexSt, ArkSt, GaSo),
                TexSt.Create(LT, NT, ULM, USM),
                ULM.Create(UAB, NT, ULL, Coastal),
                ArkSt.Create(LT, TexSt, ULM, USM),
                USM.Create(UAB, ULL, ULM, Marshall),

                Troy.Create(Marshall, GSU, USA, ArkSt),
                Marshall.Create(GSU, ODU, USA, Coastal),
                GSU.Create(USA, GaSo, AppSt, UAB),
                ODU.Create(Troy, GSU, AppSt, Coastal),
                USA.Create(ODU, GaSo, AppSt, NT),
                GaSo.Create(Troy, Marshall, ODU, AppSt),
                AppSt.Create(Troy, Marshall, Coastal, TexSt),
                Coastal.Create(Troy, GSU, USA, GaSo),

            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UAB.Create(LT, ULL, TexSt, ArkSt),
                LT.Create(NT, ULM, USM, GaSo),
                NT.Create(UAB, ULL, ArkSt, USM),
                ULL.Create(LT, TexSt, ArkSt, Coastal),
                TexSt.Create(LT, NT, ULM, USM),
                ULM.Create(UAB, NT, ULL, Marshall),
                ArkSt.Create(LT, TexSt, ULM, USM),
                USM.Create(UAB, ULL, ULM, ODU),

                Troy.Create(Marshall, GSU, USA, TexSt),
                Marshall.Create(GSU, ODU, USA, Coastal),
                GSU.Create(USA, GaSo, AppSt, ArkSt),
                ODU.Create(Troy, GSU, AppSt, Coastal),
                USA.Create(ODU, GaSo, AppSt, UAB),
                GaSo.Create(Troy, Marshall, ODU, AppSt),
                AppSt.Create(Troy, Marshall, Coastal, NT),
                Coastal.Create(Troy, GSU, USA, GaSo),

            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UAB.Create(LT, ULL, TexSt, ArkSt),
                LT.Create(NT, ULM, USM, Coastal),
                NT.Create(UAB, ULL, ArkSt, USM),
                ULL.Create(LT, TexSt, ArkSt, Marshall),
                TexSt.Create(LT, NT, ULM, USM),
                ULM.Create(UAB, NT, ULL, ODU),
                ArkSt.Create(LT, TexSt, ULM, USM),
                USM.Create(UAB, ULL, ULM, GaSo),

                Troy.Create(Marshall, GSU, USA, NT),
                Marshall.Create(GSU, ODU, USA, Coastal),
                GSU.Create(USA, GaSo, AppSt, TexSt),
                ODU.Create(Troy, GSU, AppSt, Coastal),
                USA.Create(ODU, GaSo, AppSt, ArkSt),
                GaSo.Create(Troy, Marshall, ODU, AppSt),
                AppSt.Create(Troy, Marshall, Coastal, UAB),
                Coastal.Create(Troy, GSU, USA, GaSo),

            }.Create();
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UAB.Create(LT, NT, ArkSt, Coastal),
                LT.Create(TexSt, ULM, ArkSt, USM),
                NT.Create(LT, ULL, ArkSt, Marshall),
                ULL.Create(UAB, LT, ArkSt, USM),
                TexSt.Create(UAB, NT, ULL, ODU),
                ULM.Create(UAB, NT, ULL, TexSt),
                ArkSt.Create(TexSt, ULM, USM, GaSo),
                USM.Create(UAB, NT, TexSt, ULM),

                Troy.Create(GSU, USA, AppSt, LT),
                Marshall.Create(Troy, GSU, ODU, Coastal),
                GSU.Create(ODU, USA, GaSo, ULL),
                ODU.Create(Troy, GaSo, AppSt, Coastal),
                USA.Create(Marshall, ODU, AppSt, ULM),
                GaSo.Create(Troy, Marshall, USA, AppSt),
                AppSt.Create(Marshall, GSU, Coastal, USM),
                Coastal.Create(Troy, GSU, USA, GaSo),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateF()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UAB.Create(LT, NT, ArkSt, Marshall),
                LT.Create(TexSt, ULM, ArkSt, USM),
                NT.Create(LT, ULL, ArkSt, ODU),
                ULL.Create(UAB, LT, ArkSt, USM),
                TexSt.Create(UAB, NT, ULL, GaSo),
                ULM.Create(UAB, NT, ULL, TexSt),
                ArkSt.Create(TexSt, ULM, USM, Coastal),
                USM.Create(UAB, NT, TexSt, ULM),

                Troy.Create(GSU, USA, AppSt, USM),
                Marshall.Create(Troy, GSU, ODU, Coastal),
                GSU.Create(ODU, USA, GaSo, LT),
                ODU.Create(Troy, GaSo, AppSt, Coastal),
                USA.Create(Marshall, ODU, AppSt, ULL),
                GaSo.Create(Troy, Marshall, USA, AppSt),
                AppSt.Create(Marshall, GSU, Coastal, ULM),
                Coastal.Create(Troy, GSU, USA, GaSo),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateG()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UAB.Create(LT, NT, ArkSt, ODU),
                LT.Create(TexSt, ULM, ArkSt, USM),
                NT.Create(LT, ULL, ArkSt, GaSo),
                ULL.Create(UAB, LT, ArkSt, USM),
                TexSt.Create(UAB, NT, ULL, Coastal),
                ULM.Create(UAB, NT, ULL, TexSt),
                ArkSt.Create(TexSt, ULM, USM, Marshall),
                USM.Create(UAB, NT, TexSt, ULM),

                Troy.Create(GSU, USA, AppSt, ULL),
                Marshall.Create(Troy, GSU, ODU, Coastal),
                GSU.Create(ODU, USA, GaSo, ULM),
                ODU.Create(Troy, GaSo, AppSt, Coastal),
                USA.Create(Marshall, ODU, AppSt, USM),
                GaSo.Create(Troy, Marshall, USA, AppSt),
                AppSt.Create(Marshall, GSU, Coastal, LT),
                Coastal.Create(Troy, GSU, USA, GaSo),
            }.Create();
        }
        public static Dictionary<int, int[]> CreateH()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UAB.Create(LT, NT, ArkSt, GaSo),
                LT.Create(TexSt, ULM, ArkSt, USM),
                NT.Create(LT, ULL, ArkSt, Coastal),
                ULL.Create(UAB, LT, ArkSt, USM),
                TexSt.Create(UAB, NT, ULL, Marshall),
                ULM.Create(UAB, NT, ULL, TexSt),
                ArkSt.Create(TexSt, ULM, USM, ODU),
                USM.Create(UAB, NT, TexSt, ULM),

                Troy.Create(GSU, USA, AppSt, USM),
                Marshall.Create(Troy, GSU, ODU, Coastal),
                GSU.Create(ODU, USA, GaSo, LT),
                ODU.Create(Troy, GaSo, AppSt, Coastal),
                USA.Create(Marshall, ODU, AppSt, ULL),
                GaSo.Create(Troy, Marshall, USA, AppSt),
                AppSt.Create(Marshall, GSU, Coastal, ULM),
                Coastal.Create(Troy, GSU, USA, GaSo),
            }.Create();
        }
#elif false// ODU in the mix, no UTSA
        // 14 team sbc like real life
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USA.Create(ULL, LT, TexSt, Marshall),
                ULM.Create(USA, ULL, TexSt, ODU),
                ArkSt.Create(USA, ULM, TexSt, GaSo),
                ULL.Create(ArkSt, NT, LT, AppSt),
                NT.Create(USA, ULM, ArkSt, GSU),
                LT.Create(ULM, ArkSt,NT, Coastal),
                TexSt.Create(ULL, NT, LT, Troy),

                Troy.Create(USA, Marshall, AppSt, GSU),
                Marshall.Create(ULM, ODU, GSU, Coastal),
                ODU.Create(ArkSt, Troy, GaSo, AppSt),
                GaSo.Create(ULL, Troy, Marshall, AppSt),
                AppSt.Create(NT, Marshall, GSU, Coastal),
                GSU.Create(LT, ODU, GaSo, Coastal),
                Coastal.Create(TexSt, Troy, ODU, GaSo),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USA.Create(ULL, LT, TexSt, ODU),
                ULM.Create(USA, ULL, TexSt, GaSo),
                ArkSt.Create(USA, ULM, TexSt, AppSt),
                ULL.Create(ArkSt, NT, LT, GSU),
                NT.Create(USA, ULM, ArkSt, Coastal),
                LT.Create(ULM, ArkSt,NT, Troy),
                TexSt.Create(ULL, NT, LT, Marshall),

                Troy.Create(USA, Marshall, AppSt, GSU),
                Marshall.Create(ArkSt, ODU, GSU, Coastal),
                ODU.Create(ULL, Troy, GaSo, AppSt),
                GaSo.Create(NT, Troy, Marshall, AppSt),
                AppSt.Create(LT, Marshall, GSU, Coastal),
                GSU.Create(TexSt, ODU, GaSo, Coastal),
                Coastal.Create(ULM, Troy, ODU, GaSo),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USA.Create(ULL, LT, TexSt, GaSo),
                ULM.Create(USA, ULL, TexSt, AppSt),
                ArkSt.Create(USA, ULM, TexSt, GSU),
                ULL.Create(ArkSt, NT, LT, Coastal),
                NT.Create(USA, ULM, ArkSt, Troy),
                LT.Create(ULM, ArkSt, NT, Marshall),
                TexSt.Create(ULL, NT, LT, ODU),

                Troy.Create(USA, Marshall, AppSt, GSU),
                Marshall.Create(ULL, ODU, GSU, Coastal),
                ODU.Create(NT, Troy, GaSo, AppSt),
                GaSo.Create(LT, Troy, Marshall, AppSt),
                AppSt.Create(TexSt, Marshall, GSU, Coastal),
                GSU.Create(ULM, ODU, GaSo, Coastal),
                Coastal.Create(ArkSt, Troy, ODU, GaSo),
            }.Create();
        }
        public static Dictionary<int, int[]> CreateD() => null;
        public static Dictionary<int, int[]> CreateE() => null;
        public static Dictionary<int, int[]> CreateF() => null;

#elif false
        // 14 team sbc like real life
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(ULM, ULL, TexSt, Marshall),
                NT.Create(ArkSt, TexSt, LT, Troy),
                ULM.Create(NT, UTSA, ULL, AppSt),
                UTSA.Create(ArkSt, NT, ULL, Coastal),
                ULL.Create(NT, TexSt, LT, GSU),
                TexSt.Create(ULM, UTSA, LT, USA),
                LT.Create(ArkSt, ULM, UTSA, GaSo),
                Marshall.Create(LT, Troy, Coastal, GSU),
                Troy.Create(ArkSt, AppSt, GSU, USA),
                AppSt.Create(NT, Marshall, Coastal, USA),
                Coastal.Create(ULM, Troy, USA, GaSo),
                GSU.Create(UTSA, AppSt, Coastal, GaSo),
                USA.Create(ULL, Marshall, GSU, GaSo),
                GaSo.Create(TexSt, Marshall, Troy, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(ULM, ULL, TexSt, AppSt),
                NT.Create(ArkSt, TexSt, LT, Coastal),
                ULM.Create(NT, UTSA, ULL, GSU),
                UTSA.Create(ArkSt, NT, ULL, USA),
                ULL.Create(NT, TexSt, LT, GaSo),
                TexSt.Create(ULM, UTSA, LT, Marshall),
                LT.Create(ArkSt, ULM, UTSA, Troy),
                Marshall.Create(ULL, Troy, Coastal, GSU),
                Troy.Create(TexSt, AppSt, GSU, USA),
                AppSt.Create(LT, Marshall, Coastal, USA),
                Coastal.Create(ArkSt, Troy, USA, GaSo),
                GSU.Create(NT, AppSt, Coastal, GaSo),
                USA.Create(ULM, Marshall, GSU, GaSo),
                GaSo.Create(UTSA, Marshall, Troy, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(ULM, ULL, TexSt, GSU),
                NT.Create(ArkSt, TexSt, LT, USA),
                ULM.Create(NT, UTSA, ULL, GaSo),
                UTSA.Create(ArkSt, NT, ULL, Marshall),
                ULL.Create(NT, TexSt, LT, Troy),
                TexSt.Create(ULM, UTSA, LT, AppSt),
                LT.Create(ArkSt, ULM, UTSA, Coastal),
                Marshall.Create(ULM, Troy, Coastal, GSU),
                Troy.Create(UTSA, AppSt, GSU, USA),
                AppSt.Create(ULL, Marshall, Coastal, USA),
                Coastal.Create(TexSt, Troy, USA, GaSo),
                GSU.Create(LT, AppSt, Coastal, GaSo),
                USA.Create(ArkSt, Marshall, GSU, GaSo),
                GaSo.Create(NT, Marshall, Troy, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(ULM, ULL, TexSt, GaSo),
                NT.Create(ArkSt, TexSt, LT, Marshall),
                ULM.Create(NT, UTSA, ULL, Troy),
                UTSA.Create(ArkSt, NT, ULL, AppSt),
                ULL.Create(NT, TexSt, LT, Coastal),
                TexSt.Create(ULM, UTSA, LT, GSU),
                LT.Create(ArkSt, ULM, UTSA, USA),
                Marshall.Create(ArkSt, Troy, Coastal, GSU),
                Troy.Create(NT, AppSt, GSU, USA),
                AppSt.Create(ULM, Marshall, Coastal, USA),
                Coastal.Create(UTSA, Troy, USA, GaSo),
                GSU.Create(ULL, AppSt, Coastal, GaSo),
                USA.Create(TexSt, Marshall, GSU, GaSo),
                GaSo.Create(LT, Marshall, Troy, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateF()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(ULM, ULL, TexSt, Troy),
                NT.Create(ArkSt, TexSt, LT, AppSt),
                ULM.Create(NT, UTSA, ULL, Coastal),
                UTSA.Create(ArkSt, NT, ULL, GSU),
                ULL.Create(NT, TexSt, LT, USA),
                TexSt.Create(ULM, UTSA, LT, GaSo),
                LT.Create(ArkSt, ULM, UTSA, Marshall),
                Marshall.Create(TexSt, Troy, Coastal, GSU),
                Troy.Create(LT, AppSt, GSU, USA),
                AppSt.Create(ArkSt, Marshall, Coastal, USA),
                Coastal.Create(NT, Troy, USA, GaSo),
                GSU.Create(ULM, AppSt, Coastal, GaSo),
                USA.Create(UTSA, Marshall, GSU, GaSo),
                GaSo.Create(ULL, Marshall, Troy, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateG()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(ULM, ULL, TexSt, Coastal),
                NT.Create(ArkSt, TexSt, LT, GSU),
                ULM.Create(NT, UTSA, ULL, USA),
                UTSA.Create(ArkSt, NT, ULL, GaSo),
                ULL.Create(NT, TexSt, LT, Marshall),
                TexSt.Create(ULM, UTSA, LT, Troy),
                LT.Create(ArkSt, ULM, UTSA, AppSt),
                Marshall.Create(UTSA, Troy, Coastal, GSU),
                Troy.Create(ULL, AppSt, GSU, USA),
                AppSt.Create(TexSt, Marshall, Coastal, USA),
                Coastal.Create(LT, Troy, USA, GaSo),
                GSU.Create(ArkSt, AppSt, Coastal, GaSo),
                USA.Create(NT, Marshall, GSU, GaSo),
                GaSo.Create(ULM, Marshall, Troy, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateH()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(ULM, ULL, TexSt, USA),
                NT.Create(ArkSt, TexSt, LT, GaSo),
                ULM.Create(NT, UTSA, ULL, Marshall),
                UTSA.Create(ArkSt, NT, ULL, Troy),
                ULL.Create(NT, TexSt, LT, AppSt),
                TexSt.Create(ULM, UTSA, LT, Coastal),
                LT.Create(ArkSt, ULM, UTSA, GSU),
                Marshall.Create(NT, Troy, Coastal, GSU),
                Troy.Create(ULM, AppSt, GSU, USA),
                AppSt.Create(UTSA, Marshall, Coastal, USA),
                Coastal.Create(ULL, Troy, USA, GaSo),
                GSU.Create(TexSt, AppSt, Coastal, GaSo),
                USA.Create(LT, Marshall, GSU, GaSo),
                GaSo.Create(ArkSt, Marshall, Troy, AppSt),
            }.Create();
        }

#elif false
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {TexSt, new[]{NT, ULM, LT, UAB} },
                {UTSA, new[]{TexSt, ArkSt, ULL, USM} },
                {NT, new[]{UTSA, ArkSt, ULL, USM} },
                {ArkSt, new[]{TexSt, ULM, LT, UAB} },
                {ULM, new[]{UTSA, NT, ULL, UAB} },
                {ULL, new[]{TexSt, ArkSt, LT, USM} },
                {LT, new[]{UTSA, NT, ULM, USM} },
                {USM, new[]{TexSt, ArkSt, ULM, UAB} },
                {UAB, new[]{UTSA, NT, ULL, LT} },
            };
        }

#elif false
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {TexSt, new[]{USM, ULL, ArkSt, WKU} },
                {UTSA, new[]{TexSt, LT, ULM, NT} },
                {UAB, new[]{UTSA, ULL, ArkSt, MTSU} },
                {USM, new[]{UAB, ULM, NT , WKU} },
                {LT, new[]{TexSt, USM, ArkSt, MTSU} },
                {ULL, new[]{UTSA, LT, NT, WKU} },
                {ULM, new[]{TexSt, UAB, ULL, MTSU} },
                {ArkSt, new[]{UTSA, USM, ULM, WKU} },
                {NT, new[]{TexSt, UAB, LT, ArkSt} },
                {MTSU, new[]{UTSA, USM, ULL, NT} },
                {WKU, new[]{UAB, LT, ULM, MTSU} },
            };
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                {TexSt, new[]{UAB, ULL, ArkSt, WKU} },
                {UTSA, new[]{TexSt, USM, ULM, NT} },
                {UAB, new[]{UTSA, LT, ArkSt, MTSU} },
                {USM, new[]{UAB, ULL, NT, WKU} },
                {LT, new[]{TexSt, USM, ULM, MTSU} },
                {ULL, new[]{ UTSA, LT, ArkSt, WKU} },
                {ULM, new[]{TexSt, UAB, ULL, NT} },
                {ArkSt, new[]{UTSA, USM, ULM, MTSU} },
                {NT, new[]{UAB, LT, ArkSt, WKU} },
                {MTSU, new[]{TexSt, USM, ULL, NT} },
                {WKU, new[]{UTSA, LT, ULM ,MTSU} },
            };
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new Dictionary<int, int[]>()
            {
                {TexSt, new[]{UAB, ULL, NT, WKU} },
                {UTSA, new[]{TexSt, USM, ULM, MTSU} },
                {UAB, new[]{UTSA, LT ,ArkSt, WKU} },
                {USM, new[]{TexSt, UAB, ULL, NT} },
                {LT, new[]{UTSA, USM, ULM, MTSU} },
                {ULL, new[]{ UAB, LT , ArkSt, WKU} },
                {ULM, new[]{TexSt, USM, ULL, NT} },
                {ArkSt, new[]{UTSA, LT , ULM, MTSU} },
                {NT, new[]{UAB, ULL, ArkSt, WKU} },
                {MTSU, new[]{TexSt, USM, ULM , NT} },
                {WKU, new[]{UTSA, LT, ArkSt, MTSU} },
            };
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new Dictionary<int, int[]>()
            {
                {TexSt, new[]{UAB, LT, NT, WKU} },
                {UTSA, new[]{TexSt, USM, ULL, MTSU} },
                {UAB, new[]{UTSA, LT ,ULM, WKU} },
                {USM, new[]{TexSt, UAB, ULL, ArkSt} },
                {LT, new[]{UTSA, USM, ULM, NT} },
                {ULL, new[]{ UAB, LT, ArkSt, MTSU} },
                {ULM, new[]{USM, ULL, NT, WKU} },
                {ArkSt, new[]{TexSt, LT, ULM, MTSU} },
                {NT, new[]{UTSA, ULL, ArkSt, WKU} },
                {MTSU, new[]{TexSt, UAB, ULM, NT} },
                {WKU, new[]{UTSA, USM, ArkSt, MTSU} },
            };
        }
#elif false
        public static Dictionary<int, int[]> CreateA()
        {
            return Create(
                new[]
                {
                Create(NT, LT, UTSA, ArkSt, USM),
                Create(LT, ULM, UTSA, ArkSt, USM),
                Create(ULM, NT, TexSt, ULL, WKU),
                Create(UTSA, ULM, TexSt, ULL, UAB),
                Create(ArkSt, ULM, UTSA, ULL, WKU),
                Create(TexSt, NT, LT, ArkSt, FAU),
                Create(ULL, NT, LT, TexSt, MTSU),

                Create(MTSU, NT, FAU, Troy, USA),
                Create(USM, MTSU, FAU, Troy, UAB),
                Create(FAU, ULL, Troy, WKU, UAB),
                Create(Troy, ArkSt, TexSt, WKU, USA),
                Create(WKU, MTSU, USM, UAB, USA),
                Create(UAB, ULM, MTSU, Troy, USA),
                Create(USA, LT, UTSA, USM, FAU),
                });
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return null;
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return null;
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return null;
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return null;
        }

        public static Dictionary<int, int[]> CreateF()
        {
            return null;
        }

        private static KeyValuePair<int, int[]> Create(params int[] values)
        {
            var key = values[0];
            var value = values.Skip(1).ToArray();
            return new KeyValuePair<int, int[]>(key, value);
        }

        private static Dictionary<int, int[]> Create(KeyValuePair<int, int[]>[] kvps)
        {
            var dict = new Dictionary<int, int[]>();
            foreach( var kvp in kvps)
            {
                dict.Add(kvp.Key, kvp.Value);
            }
            return dict;
        }
#elif false
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Navy.Create(Army, UTSA, ULM, UAB),
                Army.Create(TexSt, ArkSt, ULL, USM),
                TexSt.Create(UTSA, ULM, LT, UAB),
                UTSA.Create(NT, ULL, LT, USM),
                NT.Create(Navy, Army, ArkSt, ULL),
                ArkSt.Create(Navy, TexSt, ULM, LT),
                ULM.Create(Army, UTSA, ULL, USM),
                ULL.Create(Navy, TexSt, LT, UAB),
                LT.Create(Navy, Army, NT, USM),
                USM.Create(TexSt, NT, ArkSt, UAB),
                UAB.Create(UTSA, NT, ArkSt, ULM),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Navy.Create(Army, ULM, USM, UAB),
                Army.Create(TexSt, UTSA, ArkSt, ULL),
                TexSt.Create(Navy, UTSA, ULM, LT),
                UTSA.Create(NT, ULL, LT, USM),
                NT.Create(Navy, TexSt, ArkSt, UAB),
                ArkSt.Create(Navy, UTSA, ULM, ULL),
                ULM.Create(Army, NT, ULL, UAB),
                ULL.Create(Navy, TexSt, LT, USM),
                LT.Create(Army, NT, ULM, USM),
                USM.Create(TexSt, NT, ArkSt, UAB),
                UAB.Create(Army, UTSA, ArkSt, LT),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Navy.Create(Army, ULM, USM, UAB),
                Army.Create(TexSt, UTSA, NT, ULL),
                TexSt.Create(Navy, UTSA, ArkSt, LT),
                UTSA.Create(Navy, NT, LT, USM),
                NT.Create(TexSt, ArkSt, ULL, UAB),
                ArkSt.Create(Navy, UTSA, ULM, ULL),
                ULM.Create(Army, UTSA, NT, ULL),
                ULL.Create(TexSt, LT, USM, UAB),
                LT.Create(Navy, ArkSt, ULM, USM),
                USM.Create(Army, NT, ULM, UAB),
                UAB.Create(Army, TexSt, ArkSt, LT),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Navy.Create(Army, NT, USM, UAB),
                Army.Create(TexSt, UTSA, NT, LT),
                TexSt.Create(Navy, UTSA, ArkSt, USM),
                UTSA.Create(Navy, NT, ULL, UAB),
                NT.Create(TexSt, ArkSt, ULL, LT),
                ArkSt.Create(Army, UTSA, ULM, ULL),
                ULM.Create(TexSt, UTSA, NT, ULL),
                ULL.Create(Navy, LT, USM, UAB),
                LT.Create(Navy, ArkSt, ULM, USM),
                USM.Create(Army, ArkSt, ULM, UAB),
                UAB.Create(Army, TexSt, ULM, LT),
            }.Create();
        }

#endif
    }
}
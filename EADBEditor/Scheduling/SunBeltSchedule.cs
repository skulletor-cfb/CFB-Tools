using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public class SunBeltSchedule
    {
        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateA, CreateA, CreateB, CreateB, CreateC, CreateC, CreateD, CreateD };
        public static Dictionary<int, HashSet<int>> SunbeltConferenceSchedule = null;
        public static Dictionary<int, int[]> ScenarioForSeason = null;

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
            var idx = (Form1.DynastyYear - 2393) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(11, RecruitingFixup.SBCId, "SunBelt");
            SunbeltConferenceSchedule = result.BuildHashSet();
            return result;
        }

        const int TexSt = 218;
        const int ArkSt = 7;
        const int NT = 64;
        const int UTSA = 232;
        const int USM = 85;
        const int UAB = 98;
        const int LT = 43;
        const int ULM = 65;
        const int ULL = 86;
        const int WKU = 211;
        const int MTSU = 53;
        const int Army = 8;
        const int Navy = 57;

#if false
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

#else
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
#endif
    }
}

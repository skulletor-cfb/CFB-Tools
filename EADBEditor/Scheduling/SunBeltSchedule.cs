using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public class SunBeltSchedule
    {
        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateA, CreateA };
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
            var idx = (Form1.DynastyYear - 2372) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(9, RecruitingFixup.SBCId, "SunBelt");
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

#if false
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {TexSt, new[]{ArkSt, ULL, USM, Army} },
                {UTSA, new[]{TexSt, ULM, LT, UAB} },
                {NT, new[]{UTSA, ArkSt, ULL, Navy} },
                {ArkSt, new[]{ULM,LT, UAB, Army} },
                {ULM, new[]{TexSt, ULL, USM, Navy} },
                {ULL, new[]{UTSA, LT, Navy, Army} },
                {LT, new[]{TexSt, NT, USM, Army} },
                {USM, new[]{UTSA, NT, ArkSt, UAB} },
                {UAB, new[]{TexSt, NT, ULM, ULL} },
                {Navy, new[]{UTSA, ArkSt, LT, UAB} },
                {Army, new[]{NT, ULM, USM, Navy} },
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

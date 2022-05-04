using System;
using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor
{
    public class SunBeltSchedule
    {
        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateD, CreateD, CreateC, CreateC, CreateA, CreateA, CreateB, CreateB, };
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
            var idx = (Form1.DynastyYear - 2422) % Creators.Length;
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
        const int Troy = 143;
        const int USA = 235;
        const int FAU = 229;

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

#elif true
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
#else
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
#endif
    }
}

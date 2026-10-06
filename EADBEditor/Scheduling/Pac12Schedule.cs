using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public class Pac12Schedule
    {
        private static bool initRun = false;

        private static Func<Dictionary<int, int[]>>[] CorrectCreators = new Func<Dictionary<int, int[]>>[]
        {
            CreateD, CreateD,
            CreateC, CreateC,
            CreateB, CreateB,
            CreateA, CreateA,
        };

        public static Dictionary<int, HashSet<int>> Pac12ConferenceSchedule = null;
        public static Dictionary<int, int[]> ScenarioForSeason = null;

        public static void Init()
        {
            if (!initRun)
            {
                ScenarioForSeason = CreateScenarioForSeason();
                initRun = true;
            }
        }

        public static void ProcessPac12Schedule(Dictionary<int, TeamSchedule> schedule)
        {
            schedule.ProcessSchedule(ScenarioForSeason, Pac12ConferenceSchedule, TableUtility.Pac16Id, TableUtility.Pac12);
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var creatorsToUse = CorrectCreators;
            var idx = (Form1.DynastyYear - 2581) % creatorsToUse.Length;

            var result = creatorsToUse[idx]();
            result = result.Verify(12, TableUtility.Pac16Id, "Pac12");
            Pac12ConferenceSchedule = result.BuildHashSet();
            return result;
        }

        const int Wash = 110;
        const int WSU = 111;
        const int OSU = 75;
        const int UO = 74;
        const int CU = 22;
        const int Utah = 103;
        const int Stanford = 87;
        const int Cal = 17;
        const int USC = 102;
        const int UCLA = 99;
        const int Arizona = 4;
        const int ASU = 5;

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USC.Create(UCLA, Stanford, Wash, OSU, CU),
                UCLA.Create(Cal, WSU, UO, Utah),
                Cal.Create(USC, Wash, OSU, Arizona, CU),
                Stanford.Create(UCLA, Cal, WSU, ASU),
                WSU.Create(Cal, UO, Arizona, Utah),
                Wash.Create(Stanford, WSU, OSU, ASU, CU),
                UO.Create(USC, Stanford, Wash,ASU),
                OSU.Create(UCLA, WSU, UO, Arizona, Utah),
                ASU.Create(UCLA, Cal, OSU, Arizona, Utah),
                Arizona.Create(USC, Stanford, UO, CU),
                Utah.Create(USC, Stanford, Wash, Arizona),
                CU.Create(UCLA, WSU, UO, ASU, Utah),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USC.Create(UCLA, Stanford, UO, ASU, Utah),
                UCLA.Create(Cal, WSU, OSU, Arizona, CU),
                Cal.Create(USC, Wash, UO, ASU),
                Stanford.Create(UCLA, Cal, OSU, Utah),
                WSU.Create(USC, Cal, UO, ASU, CU),
                Wash.Create(UCLA, Stanford, WSU, OSU, Arizona),
                UO.Create(Stanford, Wash, ASU, Utah),
                OSU.Create(USC, WSU, UO, Arizona, CU),
                ASU.Create(UCLA, Wash, Arizona, Utah),
                Arizona.Create(Cal, Stanford, UO, CU),
                Utah.Create(Cal, WSU, OSU, Arizona),
                CU.Create(USC, Stanford, Wash, ASU, Utah),
            }.Create();
        }
        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USC.Create(UCLA, Stanford, Wash, ASU, CU),
                UCLA.Create(Cal, WSU, UO, Arizona),
                Cal.Create(USC, Wash, OSU, CU),
                Stanford.Create(UCLA, Cal, OSU, Utah),
                WSU.Create(USC, Stanford, UO, ASU),
                Wash.Create(UCLA, WSU, OSU, Arizona, Utah),
                UO.Create(Cal, Stanford, Wash, Arizona, CU),
                OSU.Create(USC, WSU, UO, ASU, Utah),
                ASU.Create(UCLA, Stanford, Wash, Arizona, Utah),
                Arizona.Create(USC, Cal, WSU, CU),
                Utah.Create(UCLA, Cal, UO, Arizona),
                CU.Create(Stanford, WSU, OSU, ASU, Utah),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USC.Create(),
                UCLA.Create(),
                Cal.Create(),
                Stanford.Create(),
                WSU.Create(),
                Wash.Create(),
                UO.Create(),
                OSU.Create(),
                ASU.Create(),
                Arizona.Create(),
                Utah.Create(),
                CU.Create(),
            }.Create();
        }
    }
}
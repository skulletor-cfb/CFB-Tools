using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EA_DB_Editor
{
    public class Pac12Schedule
    {
        private static bool initRun = false;

        private static Func<Dictionary<int, int[]>>[] CorrectCreators = new Func<Dictionary<int, int[]>>[]
        {
            CreateA, CreateA,
            CreateB, CreateB,
            CreateC, CreateC,
            CreateD, CreateD,
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
            schedule.ProcessSchedule(ScenarioForSeason, Pac12ConferenceSchedule, RecruitingFixup.Pac16Id, RecruitingFixup.Pac12);
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var creatorsToUse = CorrectCreators;
            var idx = (Form1.DynastyYear - 2563) % creatorsToUse.Length;

            var result = creatorsToUse[idx]();
            result = result.Verify(12, RecruitingFixup.Pac16Id, "Pac12");
            Pac12ConferenceSchedule = result.BuildHashSet();
            return result;
        }

        const int Wash = 110;
        const int WSU = 111;
        const int OSU = 75;
        const int UO = 74;
        const int BYU = 16;
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
                USC.Create(UCLA, Stanford, Wash, OSU, BYU),
                UCLA.Create(Cal, WSU, UO, Utah),
                Cal.Create(USC, Wash, OSU, Arizona, BYU),
                Stanford.Create(UCLA, Cal, WSU, ASU),
                WSU.Create(Cal, UO, Arizona, Utah),
                Wash.Create(Stanford, WSU, OSU, ASU, BYU),
                UO.Create(USC, Stanford, Wash,ASU),
                OSU.Create(UCLA, WSU, UO, Arizona, Utah),
                ASU.Create(UCLA, Cal, OSU, Arizona, Utah),
                Arizona.Create(USC, Stanford, UO, BYU),
                Utah.Create(USC, Stanford, Wash, Arizona),
                BYU.Create(UCLA, WSU, UO, ASU, Utah),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
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
                BYU.Create(),
            }.Create();
        }
        public static Dictionary<int, int[]> CreateC()
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
                BYU.Create(),
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
                BYU.Create(),
            }.Create();
        }
    }
}
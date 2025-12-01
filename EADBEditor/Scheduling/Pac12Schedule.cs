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
            CreateA, CreateB,
            CreateX, CreateY,
            CreateB, CreateA,
            CreateY, CreateX,
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
            var idx = (Form1.DynastyYear - 2483) % creatorsToUse.Length;

            var result = creatorsToUse[idx]();
            result = result.Verify(12, RecruitingFixup.Pac16Id, "Pac12");
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

        // Pac 12 no division setup (pac nw/cal teams all play eachother.  cal-pnw 50%, against AZ/UT 75% (8 year cycle)
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wash.Create(WSU, CU, Stanford, Arizona),
                WSU.Create(OSU, Utah, Cal, ASU),
                OSU.Create(Wash, UO, CU, USC),
                UO.Create(Wash, WSU, Utah, UCLA),
                CU.Create(WSU, UO, Utah, Arizona),
                Utah.Create(Wash, OSU, UCLA, ASU),

                Stanford.Create(OSU, CU, Cal, UCLA),
                Cal.Create(UO, Utah, USC, ASU),
                USC.Create(Wash, CU, Stanford, UCLA),
                UCLA.Create(WSU, Cal, Arizona, ASU),
                Arizona.Create(OSU, Stanford, Cal, USC),
                ASU.Create(UO, Stanford, USC, Arizona),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wash.Create(WSU, CU, Cal , ASU),
                WSU.Create(OSU, Utah, Stanford, USC),
                OSU.Create(Wash, UO, CU, UCLA),
                UO.Create(Wash, WSU, Utah, Arizona),
                CU.Create(WSU, UO, Utah, ASU),
                Utah.Create(Wash, OSU, USC, Arizona),

                Stanford.Create(UO, Utah, Cal, UCLA),
                Cal.Create(OSU, CU, USC, ASU),
                USC.Create(UO, Stanford, UCLA, ASU),
                UCLA.Create(Wash, CU, Cal , Arizona),
                Arizona.Create(WSU, Stanford, Cal, USC),
                ASU.Create(OSU, Stanford, UCLA, Arizona),
            }.Create();
        }
        public static Dictionary<int, int[]> CreateX()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wash.Create(WSU, ASU, Stanford, Utah),
                WSU.Create(OSU, Arizona, Cal, CU),
                OSU.Create(Wash, UO, ASU, USC),
                UO.Create(Wash, WSU, Arizona, UCLA),
                ASU.Create(WSU, UO, Arizona, Utah),
                Arizona.Create(Wash, OSU, UCLA, CU),

                Stanford.Create(OSU, ASU, Cal, UCLA),
                Cal.Create(UO, Arizona, USC, CU),
                USC.Create(Wash, ASU, Stanford, UCLA),
                UCLA.Create(WSU, Cal, Utah, CU),
                Utah.Create(OSU, Stanford, Cal, USC),
                CU.Create(UO, Stanford, USC, Utah),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateY()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wash.Create(WSU, ASU, Cal , CU),
                WSU.Create(OSU, Arizona, Stanford, USC),
                OSU.Create(Wash, UO, ASU, UCLA),
                UO.Create(Wash, WSU, Arizona, Utah),
                ASU.Create(WSU, UO, Arizona, CU),
                Arizona.Create(Wash, OSU, USC, Utah),

                Stanford.Create(UO, Arizona, Cal, UCLA),
                Cal.Create(OSU, ASU, USC, CU),
                USC.Create(UO, Stanford, UCLA, CU),
                UCLA.Create(Wash, ASU, Cal , Utah),
                Utah.Create(WSU, Stanford, Cal, USC),
                CU.Create(OSU, Stanford, UCLA, Utah),
            }.Create();
        }
    }
}
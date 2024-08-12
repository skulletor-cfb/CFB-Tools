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

            if( Form1.DynastyYear == 2515)
            {
                throw new Exception("Pac 12 moves to 9 game schedule!!");
            }

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

#if true // Pac 12 no division setup (pac nw/cal teams all play eachother.  cal-pnw 50%, against AZ/UT 75% (8 year cycle)
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wash.Create(WSU, BYU, Stanford, Arizona),
                WSU.Create(OSU, Utah, Cal, ASU),
                OSU.Create(Wash, UO, BYU, USC),
                UO.Create(Wash, WSU, Utah, UCLA),
                BYU.Create(WSU, UO, Utah, Arizona),
                Utah.Create(Wash, OSU, UCLA, ASU),

                Stanford.Create(OSU, BYU, Cal, UCLA),
                Cal.Create(UO, Utah, USC, ASU),
                USC.Create(Wash, BYU, Stanford, UCLA),
                UCLA.Create(WSU, Cal, Arizona, ASU),
                Arizona.Create(OSU, Stanford, Cal, USC),
                ASU.Create(UO, Stanford, USC, Arizona),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wash.Create(WSU, BYU, Cal , ASU),
                WSU.Create(OSU, Utah, Stanford, USC),
                OSU.Create(Wash, UO, BYU, UCLA),
                UO.Create(Wash, WSU, Utah, Arizona),
                BYU.Create(WSU, UO, Utah, ASU),
                Utah.Create(Wash, OSU, USC, Arizona),

                Stanford.Create(UO, Utah, Cal, UCLA),
                Cal.Create(OSU, BYU, USC, ASU),
                USC.Create(UO, Stanford, UCLA, ASU),
                UCLA.Create(Wash, BYU, Cal , Arizona),
                Arizona.Create(WSU, Stanford, Cal, USC),
                ASU.Create(OSU, Stanford, UCLA, Arizona),
            }.Create();
        }
        public static Dictionary<int, int[]> CreateX()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wash.Create(WSU, ASU, Stanford, Utah),
                WSU.Create(OSU, Arizona, Cal, BYU),
                OSU.Create(Wash, UO, ASU, USC),
                UO.Create(Wash, WSU, Arizona, UCLA),
                ASU.Create(WSU, UO, Arizona, Utah),
                Arizona.Create(Wash, OSU, UCLA, BYU),

                Stanford.Create(OSU, ASU, Cal, UCLA),
                Cal.Create(UO, Arizona, USC, BYU),
                USC.Create(Wash, ASU, Stanford, UCLA),
                UCLA.Create(WSU, Cal, Utah, BYU),
                Utah.Create(OSU, Stanford, Cal, USC),
                BYU.Create(UO, Stanford, USC, Utah),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateY()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wash.Create(WSU, ASU, Cal , BYU),
                WSU.Create(OSU, Arizona, Stanford, USC),
                OSU.Create(Wash, UO, ASU, UCLA),
                UO.Create(Wash, WSU, Arizona, Utah),
                ASU.Create(WSU, UO, Arizona, BYU),
                Arizona.Create(Wash, OSU, USC, Utah),

                Stanford.Create(UO, Arizona, Cal, UCLA),
                Cal.Create(OSU, ASU, USC, BYU),
                USC.Create(UO, Stanford, UCLA, BYU),
                UCLA.Create(Wash, ASU, Cal , Utah),
                Utah.Create(WSU, Stanford, Cal, USC),
                BYU.Create(OSU, Stanford, UCLA, Utah),
            }.Create();
        }
#elif false
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wash.Create(WSU, BYU, Stanford, Arizona),
                WSU.Create(OSU, Utah, Cal, ASU),
                OSU.Create(Wash, UO, BYU, USC),
                UO.Create(Wash, WSU, Utah, UCLA),
                BYU.Create(WSU, UO, Utah, Arizona),
                Utah.Create(Wash, OSU, UCLA, ASU),
                Stanford.Create(OSU, BYU, Cal, UCLA),
                Cal.Create(UO, Utah, USC, ASU),
                USC.Create(Wash, BYU, Stanford, UCLA),
                UCLA.Create(WSU, Cal, Arizona, ASU),
                Arizona.Create(OSU, Stanford, Cal, USC),
                ASU.Create(UO, Stanford, USC, Arizona),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wash.Create(WSU, BYU, Cal , ASU),
                WSU.Create(OSU, Utah, Stanford, USC),
                OSU.Create(Wash, UO, BYU, UCLA),
                UO.Create(Wash, WSU, Utah, Arizona),
                BYU.Create(WSU, UO, Utah, ASU),
                Utah.Create(Wash, OSU, USC, Arizona),
                Stanford.Create(UO, Utah, Cal, UCLA),
                Cal.Create(OSU, BYU, USC, ASU),
                USC.Create(UO, Stanford, UCLA, ASU),
                UCLA.Create(Wash, BYU, Cal , Arizona),
                Arizona.Create(WSU, Stanford, Cal, USC),
                ASU.Create(OSU, Stanford, UCLA, Arizona),
            }.Create();
        }
#else
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                { 110, new[]{111,87,102,4 } },
                { 111, new[]{75,17,103,99 } },
                { 75, new[]{110,74,87,102 } },
                { 74, new[]{110,111,17,5 } },
                { 87, new[]{111,74,17,16 } },
                { 17, new[]{110,75,5,103 } },
                { 102, new[]{87,99,5,103 } },
                { 99, new[]{74,17,4,16} },
                { 4, new[]{75,87,102,103} },
                { 5, new[]{111,99,4,16} },
                { 16, new[]{110,75,102,4} },
                { 103, new[]{74,99,5,16} },
            };
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                { 110, new[]{111,87,5,103 } },
                { 111, new[]{75,17,102,4 } },
                { 75, new[]{110,74,87,99 } },
                { 74, new[]{110,111,17,4 } },
                { 87, new[]{111,74,17,5 } },
                { 17, new[]{110,75,102,16 } },
                { 102, new[]{74,99,5,16 } },
                { 99, new[]{110,87,4,103} },
                { 4, new[]{17,16,102,103} },
                { 5, new[]{75,99,103,4} },
                { 16, new[]{111,74,99,5} },
                { 103, new[]{75,87,102,16} },
            };
        }
#endif
    }
}
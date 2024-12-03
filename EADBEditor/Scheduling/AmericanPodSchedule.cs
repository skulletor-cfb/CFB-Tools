using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    public static class AmericanPodSchedule
    {
#if false
        const int Cincy = 20;
        const int Memphis = 48;
        const int SMU = 83;
        const int UNCC = 100;
        const int UCF = 18;
        const int USF = 144;
        const int ECU = 25;
        const int Temple = 90;
        const int Tulsa = 97;
        const int Rice = 79;
        const int Houston = 33;
        const int Tulane = 96;
        const int UAB = 98;
        const int USM = 85;
        const int FAU = 229;
        const int UTSA = 232;

        private static bool initRun = false;
        private static bool americanPodSchedule = false;
        public static Dictionary<int, HashSet<int>> AmericanConferenceSchedule = null;
        public static Dictionary<int, int[]> ScenarioForSeason = null;

        public static void Init()
        {
            americanPodSchedule = RecruitingFixup.American.Length == 16;

            if (!americanPodSchedule)
            {
                AmericanSchedule.Init();

                if (!initRun)
                {
                    ScenarioForSeason = AmericanSchedule.CreateScenarioForSeason();
                    initRun = true;
                }

                return;
            }

            if (!initRun)
            {
                ScenarioForSeason = CreateScenarioForSeason();
                initRun = true;
            }
        }

        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            Dictionary<int, int[]> result = null;
            if (RecruitingFixup.TeamAndDivision[83] == RecruitingFixup.TeamAndDivision[18])
                result = CreateA();
            else if (RecruitingFixup.TeamAndDivision[83] == RecruitingFixup.TeamAndDivision[98])
                result = CreateB();
            else if (RecruitingFixup.TeamAndDivision[83] == RecruitingFixup.TeamAndDivision[20])
                result = CreateC();
            else
                throw new Exception("THIS SHOULDNT HAPPEN");

            var dict = result.Verify(16, RecruitingFixup.AmericanId, "American");

            AmericanConferenceSchedule = dict.BuildHashSet();
            return dict;
        }

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                SMU.Create(Houston, Tulsa, UCF, ECU),
                Houston.Create(Tulsa, USF, UNCC, UTSA),
                Tulsa.Create(Rice, USF, ECU, UNCC),
                Rice.Create(SMU, Houston, USF, Tulane),
                UCF.Create(Houston, Tulsa, Rice, ECU),
                USF.Create(SMU, UCF, UNCC, Temple),
                ECU.Create(Houston,Rice,USF, UNCC),
                UNCC.Create(SMU, Rice, UCF, Cincy),

                UAB.Create(UTSA, Memphis, Cincy, SMU),
                UTSA.Create(Memphis, Tulane, FAU, Temple),
                Memphis.Create(FAU, USM, Cincy, Tulsa),
                Tulane.Create(UAB, Memphis, Temple, USM),
                FAU.Create(UAB, Tulane, Cincy, UCF),
                Temple.Create(UAB, Memphis, FAU, USM),
                USM.Create(UAB, UTSA, FAU, ECU),
                Cincy.Create(UTSA, Tulane, Temple, USM),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                SMU.Create(Houston, Tulsa, Tulane, Memphis),
                Houston.Create(Tulsa, UAB, Tulane, FAU),
                Tulsa.Create(Rice, UAB, USM, Memphis),
                Rice.Create(SMU, Houston, USM, USF),
                UAB.Create(SMU, Rice, Tulane, Memphis),
                USM.Create(SMU, Houston, UAB, ECU),
                Tulane.Create(Tulsa, Rice, USM, Memphis),
                Memphis.Create(Houston, Rice, USM, Temple),

                UTSA.Create(FAU, UCF, Temple, SMU),
                FAU.Create(USF, UNCC, ECU, Cincy),
                UCF.Create(FAU, ECU, Temple, Tulsa),
                USF.Create(UTSA, UCF, UNCC, Cincy),
                UNCC.Create(UTSA, UCF, Cincy, UAB),
                ECU.Create(UTSA, USF, UNCC, Cincy),
                Cincy.Create(UTSA, UCF, Temple, Tulane),
                Temple.Create(FAU,USF,UNCC, ECU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC() => null;

#endif
    }
}
using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public class AmericanSchedule
    {
        private static bool initRun = false;

        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] {
            CreateB, CreateA,
            CreateA, CreateB,
        };


        public static Dictionary<int, HashSet<int>> AmericanConferenceSchedule = null;
        public static Dictionary<int, int[]> ScenarioForSeason = null;

        public static void Init()
        {
            if (!initRun)
            {
                ScenarioForSeason = CreateScenarioForSeason();
                initRun = true;
            }
        }

        public static void ProcessAmericanSchedule(Dictionary<int, TeamSchedule> schedule)
        {
            schedule.ProcessSchedule(ScenarioForSeason, AmericanConferenceSchedule, RecruitingFixup.AmericanId, RecruitingFixup.American);
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            Dictionary<int, int[]> result = null;
            var currYear = Form1.DynastyYear;

            switch (currYear)
            {
                default:
                    var idx = (Form1.DynastyYear - 2566) % Creators.Length;
                    result = Creators[idx]();
                    break;
            }

            result = result.Verify(14, RecruitingFixup.AmericanId, "American");
            AmericanConferenceSchedule = result.BuildHashSet();
            return result;
        }

        const int UAB = 98;
        const int Memphis = 48;
        const int CLT = 100;
        const int USF = 144;
        const int ECU = 25;
        const int Temple = 90;
        const int Tulsa = 97;
        const int Rice = 79;
        const int NT = 64;
        const int Tulane = 96;
        const int FAU = 229;
        const int UTSA = 232;
        const int Army = 8;
        const int Navy = 57;


        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                NT.Create(Tulsa, UAB, Army, ECU),
                UTSA.Create(NT,Tulsa, Memphis, Navy),
                Rice.Create(UTSA, Tulane, UAB, Temple),
                Tulsa.Create(Rice, Memphis, Navy, ECU),
                Tulane.Create(NT, UAB, Temple, USF),
                Memphis.Create(NT, Tulane, Army, CLT),
                UAB.Create(Memphis, Navy, FAU, ECU),
                Navy.Create(Tulane,Army,USF,ECU),
                Army.Create(Rice, Temple, FAU, CLT),
                Temple.Create(UTSA, Navy, USF, CLT),
                FAU.Create(NT, Rice, Memphis, Temple),
                USF.Create(UTSA, Tulsa, UAB, FAU),
                ECU.Create(Rice, Army, FAU, CLT),
                CLT.Create(UTSA, Tulsa, Tulane, USF),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                NT.Create(Rice, Memphis, Navy, USF),
                UTSA.Create(NT, Tulsa, Tulane, Army),
                Rice.Create(UTSA, Tulane, Navy, CLT),
                Tulsa.Create(NT, Rice, Tulane, Army),
                Tulane.Create(UAB, Army, FAU, ECU),
                Memphis.Create(Rice, Tulane, Temple, ECU),
                UAB.Create(UTSA, Tulsa, Memphis, Temple),
                Navy.Create(Memphis, Army, FAU, ECU),
                Army.Create(UAB, Temple, USF, CLT),
                Temple.Create(NT, Tulsa, Navy , USF),
                FAU.Create(UTSA, Tulsa, UAB, CLT),
                USF.Create(Rice, Memphis, FAU, ECU),
                ECU.Create(UTSA, Temple, FAU, CLT),
                CLT.Create(NT, UAB, Navy, USF),
            }.Create();
        }
    }
}
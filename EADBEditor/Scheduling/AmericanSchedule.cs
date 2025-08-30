using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public class AmericanSchedule
    {
        private static bool initRun = false;

        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { 
            CreateA, CreateA, 
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
                    var idx = (Form1.DynastyYear - 2549) % Creators.Length;
                    result = Creators[idx]();
                    break;
            }

            result = result.Verify(9, RecruitingFixup.AmericanId, "American");
            AmericanConferenceSchedule = result.BuildHashSet();
            return result;
        }

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USF.Create(ECU, Tulsa, Tulane, UAB),
                ECU.Create(CLT, Tulsa, Tulane, Memphis),
                CLT.Create(USF, Temple, Rice, UAB),
                Temple.Create(USF,ECU,Rice,Memphis),
                Tulsa.Create(CLT, Temple, Tulane, UAB),
                Rice.Create(USF,ECU,Tulsa,Memphis),
                Tulane.Create(CLT, Temple, Rice, UAB),
                UAB.Create(ECU, Temple, Rice, Memphis),
                Memphis.Create(USF, CLT, Tulsa,Tulane),
            }.Create();
        }

        const int UAB = 98;
        const int Memphis = 48;
        const int CLT = 100;
        const int USF = 144;
        const int ECU = 25;
        const int Temple = 90;
        const int Tulsa = 97;
        const int Rice = 79;
        const int Tulane = 96;
    }
}
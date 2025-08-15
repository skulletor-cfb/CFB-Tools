using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public class AmericanSchedule
    {
        private static bool initRun = false;

        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] {
            CreateA, CreateB,
            CreateC, CreateA,
            CreateB, CreateC,
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
            var currYear = Form1.DynastyYear;
            var idx = (Form1.DynastyYear - 2546) % Creators.Length;

            if(Form1.DynastyYear == 2547)
            {
                // just replay A again
                idx = 0; 
            }

            var result = Creators[idx]();


            result = result.Verify(12, RecruitingFixup.AmericanId, "American");
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
        const int Houston = 33;
        const int Tulane = 96;
        const int Cincy = 20;
        const int UCF = 18;

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USF.Create(UCF, Cincy, Rice, UAB),
                UCF.Create(ECU, Tulsa, Houston, Memphis),
                ECU.Create(USF, CLT, Rice, UAB),
                CLT.Create(UCF, Cincy, Temple, Houston),
                Cincy.Create(UCF,Temple, Tulane, Memphis),
                Temple.Create(USF, ECU, Tulsa, Tulane),
                Tulsa.Create(ECU, Cincy, Houston, Memphis),
                Rice.Create(CLT, Temple, Tulsa, Tulane),
                Houston.Create(USF, Cincy, Rice, UAB),
                Tulane.Create(UCF, ECU, Houston, UAB),
                UAB.Create(CLT, Temple, Tulsa, Memphis),
                Memphis.Create(USF, CLT, Rice, Tulane),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USF.Create(UCF, CLT, Cincy, Tulane),
                UCF.Create(ECU, Temple, Tulsa, Rice),
                ECU.Create(CLT, Temple, Rice, Memphis),
                CLT.Create(UCF, Cincy, Houston, Tulane),
                Cincy.Create(ECU, Temple, Tulane, UAB),
                Temple.Create(USF, Houston, UAB, Memphis),
                Tulsa.Create(USF, CLT, Cincy, Houston),
                Rice.Create(Cincy, Temple, Tulsa, UAB),
                Houston.Create(USF, ECU, Rice, Memphis),
                Tulane.Create(UCF, Tulsa, Houston, UAB),
                UAB.Create(UCF, ECU, Tulsa, Memphis),
                Memphis.Create(USF, CLT, Rice, Tulane),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USF.Create(),
                UCF.Create(),
                ECU.Create(),
                CLT.Create(),
                Cincy.Create(),
                Temple.Create(),
                Tulsa.Create(),
                Rice.Create(),
                Houston.Create(),
                Tulane.Create(),
                UAB.Create(),
                Memphis.Create(),
            }.Create();
        }
    }
}
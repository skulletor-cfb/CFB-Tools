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
            schedule.ProcessSchedule(ScenarioForSeason, AmericanConferenceSchedule, TableUtility.AmericanId, TableUtility.American);
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            Dictionary<int, int[]> result = null;
            var currYear = Form1.DynastyYear;

            switch (currYear)
            {
                default:
                    var idx = (Form1.DynastyYear - 2587) % Creators.Length;
                    result = Creators[idx]();
                    break;
            }

            result = result.Verify(12, TableUtility.AmericanId, "American");
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
        const int Tulane = 96;
        const int Houston = 33;
        const int UCFId = 18;
        const int SMU = 83;


        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ECU.Create(Tulane, Memphis, CLT, Rice),
                Tulane.Create(UAB, USF, CLT, SMU),
                UAB.Create(Memphis, UCFId, CLT, SMU),
                Memphis.Create(Temple, UCFId, Tulsa, Rice),
                Temple.Create(ECU, Tulane, USF, Houston),
                USF.Create(UAB, Memphis, UCFId, Rice),
                UCFId.Create(ECU, Temple, Tulsa, Houston),
                Tulsa.Create(ECU, Tulane, UAB, Houston),
                CLT.Create(Temple, USF, Tulsa, SMU),
                SMU.Create(Temple, USF, Tulsa, Houston),
                Rice.Create(UAB, UCFId, CLT, SMU),
                Houston.Create(ECU, Tulane, Memphis, Rice),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ECU.Create(Tulane, USF, CLT, Houston),
                Tulane.Create(Memphis, UCFId, CLT, Rice),
                UAB.Create(ECU, Tulane, Memphis, Rice),
                Memphis.Create(Temple, Tulsa, CLT, Houston),
                Temple.Create(ECU, UAB, USF, SMU),
                USF.Create(Tulane, Memphis, UCFId, Houston),
                UCFId.Create(UAB, Temple, Tulsa, SMU),
                Tulsa.Create(ECU, Temple, USF, SMU),
                CLT.Create(UAB, UCFId, Tulsa, Rice),
                SMU.Create(ECU, Tulane, Memphis, Houston),
                Rice.Create(Temple, USF, Tulsa, SMU),
                Houston.Create(UAB, UCFId, CLT, Rice),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ECU.Create(Tulane, USF, CLT, Rice),
                Tulane.Create(UAB, Temple, UCFId, Rice),
                UAB.Create(ECU, Memphis, Tulsa, Houston),
                Memphis.Create(ECU, Tulane, Temple, SMU),
                Temple.Create(UAB, USF, CLT, Rice),
                USF.Create(UAB, UCFId, CLT, Houston),
                UCFId.Create(ECU, Memphis, Tulsa, SMU),
                Tulsa.Create(Tulane, Temple, USF, Houston),
                CLT.Create(Memphis, UCFId, Tulsa, SMU),
                SMU.Create(ECU, UAB, USF, Houston),
                Rice.Create(Memphis ,UCFId, Tulsa, SMU),
                Houston.Create(Tulane, Temple, CLT, Rice),
            }.Create();
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor
{
    public static class ScheduleHelper
    {
        public static KeyValuePair<int, int[]> Create(this int team, params int[] values)
        {
            return new KeyValuePair<int, int[]>(team, values);
        }

        public static void Create(this Dictionary<int, int[]> dict, int team, params int[] values)
        {
            var kvp = team.Create(values);
            dict[kvp.Key] = kvp.Value;
        }

        public static Dictionary<int, int[]> Create(this IEnumerable<KeyValuePair<int, int[]>> values) => values.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public class CUSASchedule
    {
        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { 
            CreateA, CreateA, 
        };
        public static Dictionary<int, HashSet<int>> CUSAConferenceSchedule = null;
        public static Dictionary<int, int[]> ScenarioForSeason = null;

        public static void Init()
        {
            if (!initRun)
            {
                ScenarioForSeason = CreateScenarioForSeason();
                initRun = true;
            }
        }

        public static void ProcessCUSASchedule(Dictionary<int, TeamSchedule> schedule)
        {
            schedule.ProcessSchedule(ScenarioForSeason, CUSAConferenceSchedule, TableUtility.CUSAId, TableUtility.CUSA);
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var idx = (Form1.DynastyYear - 2550) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(9, TableUtility.CUSAId, "CUSA");
            CUSAConferenceSchedule = result.BuildHashSet();
            return result;
        }

        const int LT = 43;
        const int WKU = 211;
        const int MTSU = 53;
        const int FAU = 229;
        const int Army = 8;
        const int Navy=  57;
        const int UTEP = 105;
        const int NT = 64;
        const int UTSA = 232;

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Army.Create(MTSU, LT, UTEP, UTSA),
                Navy.Create(Army, WKU, FAU, NT),
                MTSU.Create(Navy, LT, UTEP, NT),
                WKU.Create(Army, MTSU, LT, UTSA),
                LT.Create(Navy, FAU, UTEP, UTSA),
                FAU.Create(Army, MTSU, WKU, NT),
                UTEP.Create(Navy, WKU, FAU, UTSA),
                UTSA.Create(Navy, MTSU, FAU, NT),
                NT.Create(Army, WKU, LT, UTEP),
            }.Create();
        }
    }
}
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
            schedule.ProcessSchedule(ScenarioForSeason, CUSAConferenceSchedule, RecruitingFixup.CUSAId, RecruitingFixup.CUSA);
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var idx = (Form1.DynastyYear - 2573) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(4, RecruitingFixup.CUSAId, "CUSA", expectedGames: 2, ifNotExpectedThen: 1);
            CUSAConferenceSchedule = result.BuildHashSet();
            return result;
        }

        const int LT = 43;
        const int WKU = 211;
        const int MTSU = 53;
        const int UTEP = 105;

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                MTSU.Create(LT, UTEP),
                WKU.Create(MTSU, LT),
                LT.Create(UTEP),
                UTEP.Create( WKU),
            }.Create();
        }
    }
}
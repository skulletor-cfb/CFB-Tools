using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EA_DB_Editor
{
    public class Pac12Schedule
    {
        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateA, CreateA, CreateB, CreateB };
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

        public static void ProcessPac12Schedule(Dictionary<int, PreseasonScheduledGame[]> schedule)
        {
            schedule.ProcessSchedule(ScenarioForSeason, Pac12ConferenceSchedule, RecruitingFixup.Pac16Id, RecruitingFixup.Pac12);
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var idx = Form1.DynastyYear % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(12, RecruitingFixup.Pac16Id, "Pac12");
            Pac12ConferenceSchedule = result.BuildHashSet();
            return result;
        }


#if true
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {110,new[] {111,103,16,102 } },
                {111,new[] {75,103,99,5 } },
                {74,new[] {110,111,5,17 } },
                {75,new[] {110,74,16,4 } },
                {103,new[] {74,75,16,87 } },
                {16,new[] {111,74,99,17 } },
                {102,new[] { 75,103,99,87} },
                {99,new[] {74,4,5,17 } },
                {4,new[] {110,103,102,87 } },
                {5,new[] {16,102,4,87 } },
                {17,new[] {111,102,4,5 } },
                {87,new[] {110,75,99,17 } },
            };
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                {110,new[] {111,103,16,99 } },
                {111,new[] {75,16,102,4 } },
                {74,new[] {110,111,4,87 } },
                {75,new[] {110,74,103,5 } },
                {103,new[] {111,74,16,17 } },
                {16,new[] {74,75,102,87} },
                {102,new[] {74,99,5,87 } },
                {99,new[] {75,103,5,17} },
                {4,new[] {16,102,99,87 } },
                {5,new[] {110,103,4,17} },
                {17,new[] {110,75,102,4 } },
                {87,new[] {111,99,5,17 } },
            };
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
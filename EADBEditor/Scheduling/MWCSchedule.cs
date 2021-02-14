using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EA_DB_Editor
{
    public class MWCSchedule
    {
        private const int UNMId = 60;
        private const int NMSUId = 61;
        private const int SDSUId = 81;
        private const int HawaiiId = 32;

        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateA, CreateA, CreateB, CreateB };
        public static Dictionary<int, HashSet<int>> MWCConferenceSchedule = null;
        public static Dictionary<int, int[]> ScenarioForSeason = null;

        public static void Init()
        {
            if (!initRun)
            {
                ScenarioForSeason = CreateScenarioForSeason();
                initRun = true;
            }
        }

        public static void ProcessMWCSchedule(Dictionary<int, PreseasonScheduledGame[]> schedule)
        {
            schedule.ProcessSchedule(ScenarioForSeason, MWCConferenceSchedule, RecruitingFixup.MWCId, RecruitingFixup.MWC);
        }



        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var idx = Form1.DynastyYear % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(12, RecruitingFixup.MWCId, "MWC");
            MWCConferenceSchedule = result.BuildHashSet();
            return result;
        }


        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {HawaiiId,new[] {SDSUId,101,59,23 } },
                {SDSUId,new[] {82,101,115,NMSUId } },
                {29,new[] { HawaiiId, SDSUId,NMSUId,104 } },
                {82,new[] { HawaiiId, 29,59,UNMId } },
                {101,new[] {29,82,59,1 } },
                {59,new[] {SDSUId,29,115,104 } },
                {23,new[] { 82,101,115,1} },
                {115,new[] {29,UNMId,NMSUId,104 } },
                {UNMId,new[] { HawaiiId, 101,23,1 } },
                {NMSUId,new[] {59,23,UNMId,1 } },
                {104,new[] {SDSUId,23,UNMId,NMSUId } },
                {1,new[] { HawaiiId, 82,115,104 } },
            };
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                {HawaiiId,new[] {SDSUId,101,59,115 } },
                {SDSUId,new[] {82,59,23,UNMId } },
                {29,new[] { HawaiiId, SDSUId,UNMId,1 } },
                {82,new[] { HawaiiId, 29,101,NMSUId } },
                {101,new[] {SDSUId,29,59,104 } },
                {59,new[] {29,82,23,1} },
                {23,new[] {29,115,NMSUId,1 } },
                {115,new[] {82,101,NMSUId,104} },
                {UNMId,new[] {59,23,115,1 } },
                {NMSUId,new[] { HawaiiId, 101,UNMId,104} },
                {104,new[] { HawaiiId, 82,23,UNMId } },
                {1,new[] {SDSUId,115,NMSUId,104 } },
            };
        }
    }
}
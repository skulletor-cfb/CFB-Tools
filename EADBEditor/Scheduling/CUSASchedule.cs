using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public class CUSASchedule
    {
        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateA, CreateA, CreateC, CreateC, CreateD, CreateD, CreateB, CreateB };
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

        public static void ProcessCUSASchedule(Dictionary<int, PreseasonScheduledGame[]> schedule)
        {
            schedule.ProcessSchedule(ScenarioForSeason, CUSAConferenceSchedule, RecruitingFixup.CUSAId, RecruitingFixup.CUSA);
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var idx = (Form1.DynastyYear - 2356) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(11, RecruitingFixup.CUSAId, "CUSA");
            CUSAConferenceSchedule = result.BuildHashSet();
            return result;
        }

        const int Troy = 143;
        const int USA = 235;
        const int FAU = 229;
        const int FIU = 230;
        const int GaSo = 902;
        const int AppSt = 901;
        const int Marshall = 46;
        const int ODU = 234;
        const int Army = 8;
        const int Navy = 57;
        const int GSU = 233;

        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {Navy,new[] {Army, AppSt, GSU, Troy} },
                {Army,new[] {Marshall, GaSo, USA, FAU} },
                {Marshall,new[] {GSU, USA, Troy, FIU} },
                {ODU,new[] {Navy, Marshall, USA, FIU} },
                {AppSt,new[] {Army, ODU, Troy, FAU} },
                {GaSo,new[] {Navy, Marshall, AppSt, FAU} },
                {GSU,new[] {Army, ODU, GaSo, USA} },
                {USA,new[] {Navy, AppSt, Troy, FIU} },
                {Troy,new[] {Army, ODU, GaSo, FAU} },
                {FAU,new[] {Marshall, ODU, GSU, FIU} },
                {FIU,new[] {Navy, AppSt, GaSo, GSU} },
            };
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return null;
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return null;
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return null;
        }

    }
}
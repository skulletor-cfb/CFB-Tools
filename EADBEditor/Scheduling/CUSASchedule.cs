using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public class CUSASchedule
    {
        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateA, CreateA, CreateB, CreateB };
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
            var idx = (Form1.DynastyYear - 2372) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(12, RecruitingFixup.CUSAId, "CUSA");
            CUSAConferenceSchedule = result.BuildHashSet();
            return result;
        }

        const int FAU = 229;
        const int FIU = 230;
        const int Troy = 143;
        const int USA = 235;
        const int WKU = 211;
        const int MTSU = 53;

        const int ODU = 234;
        const int CLT = 100;
        const int GaSo = 181;
        const int AppSt = 341;
        const int GSU = 233;
        const int CCU = 61;

        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {Troy,new[] {USA,FIU,FAU,AppSt } },
                {USA,new[] {WKU,FIU,GaSo,CLT } },
                {MTSU,new[] {Troy,USA,CLT,GSU } },
                {WKU,new[] {Troy,MTSU,FAU,ODU } },
                {FIU,new[] {MTSU,WKU,FAU,CCU } },
                {FAU,new[] {USA,MTSU,GaSo,GSU } },
                {AppSt,new[] { WKU,FIU,GaSo,CCU} },
                {GaSo,new[] {MTSU,ODU,CLT,GSU } },
                {ODU,new[] {Troy,FIU,AppSt,CCU } },
                {CLT,new[] {FAU,AppSt,ODU,CCU } },
                {GSU,new[] {USA,AppSt,ODU,CLT } },
                {CCU,new[] {Troy,WKU,GaSo,GSU } },
            };
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                {Troy,new[] {USA,FIU,FAU,GaSo } },
                {USA,new[] {WKU,FAU,AppSt,ODU } },
                {MTSU,new[] {Troy,USA,ODU,CCU } },
                {WKU,new[] {Troy,MTSU,FIU,CLT } },
                {FIU,new[] {USA,MTSU,FAU,GSU } },
                {FAU,new[] {MTSU,WKU,AppSt,CCU} },
                {AppSt,new[] {MTSU,GaSo,CLT,CCU } },
                {GaSo,new[] {WKU,FIU,CLT,GSU} },
                {ODU,new[] {FAU,AppSt,GaSo,CCU } },
                {CLT,new[] {Troy,FIU,ODU,GSU} },
                {GSU,new[] {Troy,WKU,AppSt,ODU } },
                {CCU,new[] {USA,GaSo,CLT,GSU } },
            };
        }
    }
}
using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public class CUSASchedule
    {
        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateA, CreateA, CreateB, CreateB};
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
            var idx = (Form1.DynastyYear - 2395) % Creators.Length;
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
        const int GSU = 233;

        const int Army = 8;
        const int Navy = 57;
        const int ODU = 234;
        const int Charlotte = 100;
        const int GaSo = 181;
        const int AppSt = 34;
        const int Coastal = 61;

#if false
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
#elif false
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {Army,new[] {FAU, GSU, Charlotte,Coastal } },
                {Navy,new[] {Army, AppSt, GaSo, WKU} },
                {ODU,new[] {Army, Navy, GaSo, USA} },
                {Charlotte,new[] {Navy, ODU, AppSt, Troy} },
                {AppSt,new[] {Army, ODU, Coastal, FIU } },
                {Coastal,new[] {Navy, ODU, Charlotte, FIU } },
                {GaSo,new[] {Army, Charlotte, AppSt, Coastal} },

                {FAU,new[] {Coastal, WKU, USA, GSU} },
                {FIU,new[] {FAU, Troy, WKU, MTSU} },
                {Troy,new[] {AppSt, FAU, USA, GSU} },
                {USA,new[] {Charlotte, FIU, WKU, MTSU } },
                {WKU,new[] {ODU, Troy, MTSU, GSU } },
                {MTSU,new[] {Navy, GaSo, FAU, Troy } },
                {GSU,new[] {GaSo, FIU, USA, MTSU } },
            };
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                { Army,new[] { Charlotte,Coastal    , FIU, Troy}},
                { Navy,new[] { Army, AppSt, GaSo, GSU} },
                { ODU,new[] { Army, Navy, GaSo, MTSU } },
                { Charlotte,new[] { Navy, ODU, AppSt, WKU } },
                { AppSt,new[] { Army, ODU, Coastal, USA } },
                { Coastal,new[] { Navy, ODU, Charlotte, Troy } },
                { GaSo,new[] { Army, Charlotte, AppSt, Coastal } },

                { FAU,new[] { ODU, GaSo, WKU, GSU } },
                { FIU,new[] { Navy, FAU, Troy, WKU } },
                { Troy,new[] { FAU, USA, GSU, WKU } },
                { USA,new[] { Coastal, FAU, FIU, MTSU } },
                { WKU,new[] { AppSt, USA, MTSU, GSU } },
                { MTSU,new[] { Charlotte, FAU, FIU, Troy } },
                { GSU,new[] { GaSo, FIU, USA, MTSU } },
            };
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new Dictionary<int, int[]>()
            {
                { Army,new[] { Charlotte,Coastal    , USA, WKU}},
                { Navy,new[] { Army, AppSt, GaSo, Troy} },
                { ODU,new[] { Army, Navy, GaSo, GSU} },
                { Charlotte,new[] { Navy, ODU, AppSt, FIU} },
                { AppSt,new[] { Army, ODU, Coastal, MTSU } },
                { Coastal,new[] { Navy, ODU, Charlotte, WKU } },
                { GaSo,new[] { Army, Charlotte, AppSt, Coastal } },

                { FAU,new[] { Charlotte, AppSt, USA, GSU } },
                { FIU,new[] { GaSo, FAU, Troy, WKU } },
                { Troy,new[] { ODU, FAU, USA, GSU } },
                { USA,new[] { Navy, FIU, WKU, MTSU } },
                { WKU,new[] { FAU, Troy, MTSU, GSU } },
                { MTSU,new[] { Coastal, FAU, FIU, Troy } },
                { GSU,new[] { GaSo, FIU, USA, MTSU } },
            };
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new Dictionary<int, int[]>()
            {
                { Army,new[] { Charlotte,Coastal    , FAU, MTSU}},
                { Navy,new[] { Army, AppSt, GaSo, WKU} },
                { ODU,new[] { Army, Navy, GaSo, USA} },
                { Charlotte,new[] { Navy, ODU, AppSt, GSU} },
                { AppSt,new[] { Army, ODU, Coastal, FIU } },
                { Coastal,new[] { Navy, ODU, Charlotte, FIU } },
                { GaSo,new[] { Army, Charlotte, AppSt, Coastal } },

                { FAU,new[] { Coastal, USA, WKU, GSU } },
                { FIU,new[] { FAU, Troy, WKU, MTSU } },
                { Troy,new[] { AppSt, GaSo, FAU, USA } },
                { USA,new[] { Charlotte, FIU, MTSU, GSU } },
                { WKU,new[] { ODU, Troy, USA, MTSU } },
                { MTSU,new[] { Navy, Troy, FAU, GSU } },
                { GSU,new[] { GaSo, FIU, Troy, WKU   } },
            };
        }
        public static Dictionary<int, int[]> CreateE()
        {
            return new Dictionary<int, int[]>()
            {
                { Army,new[] { Charlotte,Coastal    , FIU, Troy}},
                { Navy,new[] { Army, AppSt, GaSo, FIU} },
                { ODU,new[] { Army, Navy, GaSo, MTSU} },
                { Charlotte,new[] { Navy, ODU, AppSt, WKU} },
                { AppSt,new[] { Army, ODU, Coastal, GSU } },
                { Coastal,new[] { Navy, ODU, Charlotte, USA } },
                { GaSo,new[] { Army, Charlotte, AppSt, Coastal } },

                { FAU,new[] { Navy, ODU, WKU, GSU } },
                { FIU,new[] { FAU, Troy, USA, MTSU } },
                { Troy,new[] { Coastal, FAU, USA, GSU } },
                { USA,new[] { GaSo, FAU, WKU, MTSU } },
                { WKU,new[] { AppSt, FIU, Troy, MTSU } },
                { MTSU,new[] { Charlotte, FAU, Troy, GSU } },
                { GSU,new[] { GaSo, FIU ,USA, WKU  } },
            };
        }

        public static Dictionary<int, int[]> CreateF()
        {
            return new Dictionary<int, int[]>()
            {
                { Army,new[] { Charlotte,Coastal    , USA, WKU}},
                { Navy,new[] { Army, AppSt, GaSo, Troy} },
                { ODU,new[] { Army, Navy, GaSo, FIU} },
                { Charlotte,new[] { Navy, ODU, AppSt, FIU} },
                { AppSt,new[] { Army, ODU, Coastal, MTSU } },
                { Coastal,new[] { Navy, ODU, Charlotte, GSU} },
                { GaSo,new[] { Army, Charlotte, AppSt, Coastal } },

                { FAU,new[] { Charlotte, AppSt, WKU, GSU } },
                { FIU,new[] { FAU, Troy , WKU, GSU } },
                { Troy,new[] { ODU, FAU, USA, WKU } },
                { USA,new[] { Navy, FAU, FIU, MTSU } },
                { WKU,new[] { GaSo, USA, MTSU, GSU } },
                { MTSU,new[] { Coastal, FAU, FIU, Troy } },
                { GSU,new[] { GaSo, Troy, USA, MTSU  } },
            };
        }

#else
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {GSU,new[] {GaSo, AppSt, ODU, Navy } },
                {GaSo,new[] {Coastal, AppSt, Army, FAU} },
                {Coastal,new[] {GSU, Charlotte, ODU, Navy } },
                {AppSt,new[] {Coastal, ODU, FIU, USA } },
                {Charlotte,new[] {GSU, GaSo, AppSt, Troy} },
                {ODU,new[] {GaSo, Charlotte, FIU, USA} },


                {Army,new[] {GSU, Coastal, FAU, Troy } },
                {Navy,new[] {GaSo, Army , FIU, USA } },
                {FAU,new[] {GSU, Coastal, Navy, Troy} },
                {FIU,new[] {Charlotte, Army, FAU, Troy} },
                {Troy,new[] {AppSt, ODU, Navy, USA} },
                {USA,new[] {Charlotte, Army, FAU, FIU} },
            };
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                {GSU,new[] {GaSo, AppSt, ODU, Troy} },
                {GaSo,new[] {Coastal, AppSt, FIU, USA} },
                {Coastal,new[] {GSU, Charlotte, ODU, Troy} },
                {AppSt,new[] {Coastal, ODU, Army, FAU } },
                {Charlotte,new[] {GSU, GaSo, AppSt, Navy} },
                {ODU,new[] {GaSo, Charlotte, Navy, FAU} },


                {Army,new[] {Charlotte, ODU, FIU, USA } },
                {Navy,new[] {AppSt, Army, FAU, Troy } },
                {FAU,new[] {Charlotte, Army, Troy, USA} },
                {FIU,new[] {GSU, Coastal, Navy, FAU} },
                {Troy,new[] {GaSo, Army, FIU, USA} },
                {USA,new[] {GSU, Coastal, Navy, FIU} },
            };
        }
#endif
    }
}
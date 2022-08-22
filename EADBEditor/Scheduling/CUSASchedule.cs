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

        public static Dictionary<int, int[]> Create(this IEnumerable<KeyValuePair<int, int[]>> values) => values.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public class CUSASchedule
    {
        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateA, CreateA };
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
            var idx = (Form1.DynastyYear - 2439) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(5, RecruitingFixup.CUSAId, "CUSA", expectedGames: 2);
            CUSAConferenceSchedule = result.BuildHashSet();
            return result;
        }

        const int FIU = 230;
        const int WKU = 211;
        const int MTSU = 53;
        const int Army = 8;
        const int Navy = 57;

#if true
        // 5 team CUSA
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Army.Create(WKU, FIU),
                Navy.Create(Army, MTSU),
                FIU.Create(Navy, WKU),
                WKU.Create(Navy, MTSU),
                MTSU.Create(Army ,FIU),
            }.Create();
        }
#elif false
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                GSU.Create(Marshall, GaSo, AppSt, WKU),
                Marshall.Create(Coastal, GaSo, WKU, FAU),
                Coastal.Create(GSU, GaSo, ODU, Troy),
                GaSo.Create(AppSt, ODU, MTSU, FIU),
                AppSt.Create(Marshall, Coastal, MTSU, FIU),
                ODU.Create(GSU, Marshall, AppSt, USA),
                MTSU.Create(ODU,FIU, USA, Troy),
                FIU.Create(ODU, WKU, FAU, Troy),
                USA.Create(GaSo,AppSt,FIU,FAU),
                WKU.Create(Coastal,MTSU,USA,FAU),
                FAU.Create(GSU, Coastal,MTSU,Troy),
                Troy.Create(GSU, Marshall,USA,WKU)
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                GSU.Create(Marshall, GaSo, MTSU, FIU),
                Marshall.Create(Coastal, AppSt, FIU, USA),
                Coastal.Create(GSU, GaSo, ODU, USA),
                GaSo.Create(Marshall, AppSt, ODU, WKU),
                AppSt.Create(GSU, Coastal, WKU, FAU),
                ODU.Create(GSU, Marshall, AppSt, Troy),
                MTSU.Create(Marshall, Coastal, FIU, Troy),
                FIU.Create(Coastal, USA, WKU, FAU),
                USA.Create(GSU, MTSU, WKU, FAU),
                WKU.Create(ODU, MTSU, FAU, Troy),
                FAU.Create(GaSo, ODU, MTSU, Troy),
                Troy.Create(GaSo, AppSt, FIU, USA)
            }.Create();
        }
#elif false
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

#elif false
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
#else
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {Army,new[] {ODU, GSU, Coastal, FIU } },
                {Navy,new[] {Army, Charlotte, GaSo, FIU } },
                {Charlotte,new[] {Army, ODU, AppSt, GSU} },
                {ODU,new[] {Navy, GaSo, Coastal, FIU} },
                {AppSt,new[] {Army, Navy, ODU, Coastal } },
                {GSU,new[] {Navy, ODU, AppSt, GaSo } },
                {GaSo,new[] {Army, Charlotte, AppSt, Coastal} },
                {Coastal,new[] {Navy, Charlotte, GSU, FIU } },
                {FIU,new[] {Charlotte, AppSt, GSU, GaSo} },
            };
        }
#endif
    }
}
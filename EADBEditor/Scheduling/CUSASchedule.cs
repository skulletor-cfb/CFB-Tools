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

        public static void ProcessCUSASchedule(Dictionary<int, PreseasonScheduledGame[]> schedule)
        {
            schedule.ProcessSchedule(ScenarioForSeason, CUSAConferenceSchedule, RecruitingFixup.CUSAId, RecruitingFixup.CUSA);
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var idx = (Form1.DynastyYear - 2504) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(9, RecruitingFixup.CUSAId, "CUSA", expectedGames: 5);
            CUSAConferenceSchedule = result.BuildHashSet();
            return result;
        }

        const int LT = 43;
        const int WKU = 211;
        const int MTSU = 53;
        const int FAU = 229;
        const int Troy = 143;
        const int USA = 235;
        const int GSU = 233;
        const int ODU = 234;
        const int Marshall = 46;
        const int GaSo = 181;
        const int AppSt = 34;
        const int Coastal = 61;
        const int Army = 8;
        const int Navy=  57;
        const int UTEP = 105;
        const int NT = 64;
        const int UTSA = 232;
        const int UAB = 98;
        const int USM = 85;

#if false
        // 9 team CUSA all over
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Army.Create(LT, UAB, WKU, FIU),
                Navy.Create(Army, USM, MTSU, NT),
                LT.Create(Navy, USM, WKU, FIU),
                USM.Create(Army, UAB, WKU, NT),
                UAB.Create(Navy, LT, MTSU, FIU),
                WKU.Create(Navy, UAB ,MTSU, NT),
                MTSU.Create(Army, LT, USM, NT),
                NT.Create(Army, LT, UAB, FIU),
                FIU.Create(Navy, USM, WKU, MTSU),
            }.Create();
        }

#elif false
        // 11 team CUSA eastern seaboard
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                FAU.Create(ODU, WKU, AppSt, GSU),
                FIU.Create(FAU, Marshall, Coastal, GASO),
                Army.Create(FIU, WKU, AppSt, GSU),
                Navy.Create(FAU, Army, Coastal, GASO),
                ODU.Create(FIU, Navy, AppSt, GSU),
                Marshall.Create(FAU, Army, ODU, GASO),
                WKU.Create(FIU, Navy, Marshall, GSU),
                Coastal.Create(FAU, Army, ODU, WKU),
                AppSt.Create(FIU, Navy, Marshall, Coastal),
                GASO.Create(Army, ODU, WKU, AppSt),
                GSU.Create(Navy, Marshall, Coastal, GASO),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                FAU.Create(Army, ODU, WKU, GSU),
                FIU.Create(FAU, Navy, Marshall, Coastal),
                Army.Create(FIU, ODU, WKU, AppSt),
                Navy.Create(Army, Marshall, Coastal, GASO),
                ODU.Create(Navy, WKU, AppSt, GSU),
                Marshall.Create(FAU, ODU, Coastal, GASO),
                WKU.Create(FIU, Marshall, AppSt, GSU),
                Coastal.Create(FAU, Army, WKU, GASO),
                AppSt.Create(FIU, Navy, Coastal, GSU),
                GASO.Create(FAU, Army, ODU, AppSt),
                GSU.Create(FIU, Navy, Marshall,GASO),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }
#elif false
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
#elif false // 4 team CUSA
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                MTSU.Create(LT, UTEP, Army),
                WKU.Create(MTSU, LT, Navy),
                LT.Create(UTEP, Army),
                UTEP.Create(WKU, Navy),
                Army.Create(WKU, UTEP),
                Navy.Create(Army, MTSU, LT)
            }.Create();
        }
#elif true // 9 team CUSA
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
#elif false // 12 team CUSA west/east
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Army.Create(WKU, FAU, USM, NT),
                WKU.Create(Navy, MTSU, FIU, UAB),
                FAU.Create(WKU, MTSU, USM, NT),
                Navy.Create(Army, FAU, MTSU, UTSA),
                MTSU.Create(Army, FIU, LT  , UTEP),
                FIU.Create(Army, FAU, Navy, UTSA),

                UTSA.Create(MTSU, LT, NT , UAB),
                LT.Create(Navy, FIU, UTEP, USM),
                UTEP.Create(Navy, FIU , UTSA, USM),
                USM.Create(WKU, UTSA, NT, UAB),
                NT.Create(WKU, LT, UTEP, UAB),
                UAB.Create(Army, FAU, LT, UTEP),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Army.Create(WKU, FAU, UTSA, LT),
                WKU.Create(Navy, MTSU, FIU, UTEP),
                FAU.Create(WKU, MTSU, UTSA, LT),
                Navy.Create(Army, FAU, MTSU, USM ),
                MTSU.Create(Army, FIU, NT, UAB),
                FIU.Create(Army, FAU, Navy, USM),

                UTSA.Create(WKU, LT, NT, UAB),
                LT.Create(WKU, UTEP, USM, UAB),
                UTEP.Create(Army, FAU, UTSA, USM),
                USM.Create(MTSU, UTSA, NT, UAB),
                NT.Create(Navy, FIU , LT, UTEP),
                UAB.Create(Navy, FIU, UTEP, NT),
            }.Create();
        }
#elif false // 9 team CUSA of randoms
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Army.Create(WKU, NT , FIU, LT),
                Navy.Create(Army, MTSU, FAU, UTEP),
                WKU.Create(Navy, MTSU, FAU, UTEP),
                MTSU.Create(Army, NT, FIU, LT),
                NT.Create(Navy, WKU, FIU , UTEP),
                FAU.Create(Army, MTSU, NT, LT),
                FIU.Create(Navy, WKU, FAU , UTEP),
                LT.Create(Navy, WKU, NT, FIU),
                UTEP.Create(Army, MTSU, FAU, LT),
            }.Create();
        }
#elif false // we have a 14 team CUSA , WKU/Marshall, Army/Navy cross
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Army.Create(WKU, USA, FIU, Marshall),
                WKU.Create(FAU, MTSU, Troy, AppSt),
                USA.Create(WKU, FAU, MTSU, Coastal),
                FAU.Create(Army, MTSU, Troy, GaSo),
                MTSU.Create(Army, Troy, FIU , ODU),
                Troy.Create(Army, USA, FIU, GSU),
                FIU.Create(WKU, USA, FAU, Navy),

                Navy.Create(Army, Marshall, Coastal, AppSt),
                Marshall.Create(WKU, ODU, Coastal , GaSo),
                GSU.Create(FIU, Navy, Marshall, GaSo),
                ODU.Create(Troy, Navy, GSU, AppSt),
                Coastal.Create(MTSU, GSU, ODU, GaSo),
                GaSo.Create(USA, Navy, ODU, AppSt),
                AppSt.Create(FAU, Marshall, GSU, Coastal),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }

        public static Dictionary<int, int[]> CreateF()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }
#elif false // when we have east/south schedules for CUSA

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
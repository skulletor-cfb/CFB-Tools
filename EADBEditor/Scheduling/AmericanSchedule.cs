using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public class AmericanSchedule
    {
        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateA, CreateA, CreateB, CreateB, CreateC, CreateC, CreateD, CreateD, CreateE, CreateE, CreateF,CreateF };
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

        public static void ProcessAmericanSchedule(Dictionary<int, PreseasonScheduledGame[]> schedule)
        {
             schedule.ProcessSchedule(ScenarioForSeason, AmericanConferenceSchedule, RecruitingFixup.AmericanId, RecruitingFixup.American);
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var idx = (Form1.DynastyYear - 2439) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(14, RecruitingFixup.AmericanId, "American");
            AmericanConferenceSchedule = result.BuildHashSet();
            return result;
        }

        const int CincyId = 20;
        const int MemphisId = 48;
        const int SMUId = 83;
        const int CharlotteId = 100;
        const int UCFId = 18;
        const int USFId = 144;
        const int ECUId = 25;
        const int TempleId = 90;
        const int TulsaId = 97;
        const int RiceId = 79;
        const int HoustonId = 33;
        const int TulaneId = 96;
        const int UAB = 98;
        const int USM = 85;
        const int FAU = 229;
        const int UTSA = 232;

#if true
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                CharlotteId.Create(TulaneId, SMUId, RiceId, ECUId),
                TulaneId.Create(UTSA, TulsaId, HoustonId, USM),
                UTSA.Create(CharlotteId, SMUId, RiceId, FAU),
                SMUId.Create(TulaneId, TulsaId, HoustonId, USFId),
                TulsaId.Create(CharlotteId, UTSA, RiceId, TempleId),
                RiceId.Create(TulaneId, SMUId, HoustonId, MemphisId),
                HoustonId.Create(CharlotteId, UTSA, TulsaId, UAB),

                ECUId.Create(HoustonId, UAB, TempleId, USFId),
                USM.Create(CharlotteId, ECUId, UAB, FAU),
                UAB.Create(RiceId, MemphisId, TempleId, USFId),
                MemphisId.Create(TulsaId, ECUId, USM, FAU),
                TempleId.Create(UTSA, USM, MemphisId, USFId),
                FAU.Create(SMUId, ECUId, UAB, TempleId),
                USFId.Create(TulaneId, USM, MemphisId, FAU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB() => null;
        public static Dictionary<int, int[]> CreateC() => null;
        public static Dictionary<int, int[]> CreateD() => null;
        public static Dictionary<int, int[]> CreateE() => null;
        public static Dictionary<int, int[]> CreateF() => null;
#elif false
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USM.Create(UAB, SMUId, RiceId, TulaneId),
                TulsaId.Create(USM, SMUId, RiceId, USFId),
                UAB.Create(TulsaId, MemphisId, HoustonId, TempleId),
                SMUId.Create(UAB, MemphisId, HoustonId, CharlotteId),
                MemphisId.Create(USM, TulsaId, RiceId, ECUId),
                RiceId.Create(UAB, SMUId, HoustonId, CincyId),
                HoustonId.Create(USM, TulsaId, MemphisId, TulaneId),
                TulaneId.Create(UCFId, CincyId, TempleId, USFId),
                UCFId.Create(USM, TulsaId, ECUId, TempleId),
                CincyId.Create(HoustonId, UCFId, TempleId, CharlotteId),
                ECUId.Create(RiceId, TulaneId, CincyId, CharlotteId),
                TempleId.Create(MemphisId, ECUId, CharlotteId, USFId),
                CharlotteId.Create(UAB, TulaneId, UCFId, USFId),
                USFId.Create(SMUId, UCFId, CincyId, ECUId),
            }.Create();
        }


        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USM.Create(UAB, SMUId, RiceId, TulaneId),
                TulsaId.Create(USM, SMUId, RiceId, ECUId),
                UAB.Create(TulsaId, MemphisId, HoustonId, CincyId),
                SMUId.Create(UAB, MemphisId, HoustonId, TulaneId),
                MemphisId.Create(USM, TulsaId, RiceId, USFId),
                RiceId.Create(UAB, SMUId, HoustonId, CharlotteId),
                HoustonId.Create(USM, TulsaId, MemphisId, TempleId),
                TulaneId.Create(UCFId, CincyId, TempleId, USFId),
                UCFId.Create(TulsaId, MemphisId, ECUId, TempleId),
                CincyId.Create(SMUId, UCFId, TempleId, CharlotteId),
                ECUId.Create(UAB, TulaneId, CincyId, CharlotteId),
                TempleId.Create(USM, ECUId, CharlotteId, USFId),
                CharlotteId.Create(HoustonId, TulaneId, UCFId, USFId),
                USFId.Create(RiceId, UCFId, CincyId, ECUId),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USM.Create(UAB, SMUId, RiceId, TulaneId),
                TulsaId.Create(USM, SMUId, RiceId, CharlotteId),
                UAB.Create(TulsaId, MemphisId, HoustonId, TempleId),
                SMUId.Create(UAB, MemphisId, HoustonId, ECUId),
                MemphisId.Create(USM, TulsaId, RiceId, CincyId),
                RiceId.Create(UAB, SMUId, HoustonId, TulaneId),
                HoustonId.Create(USM, TulsaId, MemphisId, USFId),
                TulaneId.Create(UCFId, CincyId, TempleId, USFId),
                UCFId.Create(TulsaId, HoustonId, ECUId, TempleId),
                CincyId.Create(RiceId, UCFId, TempleId, CharlotteId),
                ECUId.Create(MemphisId, TulaneId, CincyId, CharlotteId),
                TempleId.Create(SMUId, ECUId, CharlotteId, USFId),
                CharlotteId.Create(UAB, TulaneId, UCFId, USFId),
                USFId.Create(USM, UCFId, CincyId, ECUId),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USM.Create(UAB, SMUId, RiceId, TulaneId),
                TulsaId.Create(USM, SMUId, RiceId, TulaneId),
                UAB.Create(TulsaId, MemphisId, HoustonId, USFId),
                SMUId.Create(UAB, MemphisId, HoustonId, CharlotteId),
                MemphisId.Create(USM, TulsaId, RiceId, TempleId),
                RiceId.Create(UAB, SMUId, HoustonId, ECUId),
                HoustonId.Create(USM, TulsaId, MemphisId, CincyId),
                TulaneId.Create(UCFId, CincyId, TempleId, USFId),
                UCFId.Create(TulsaId, UAB, ECUId, TempleId),
                CincyId.Create(USM, UCFId, TempleId, CharlotteId),
                ECUId.Create(HoustonId, TulaneId, CincyId, CharlotteId),
                TempleId.Create(RiceId, ECUId, CharlotteId, USFId),
                CharlotteId.Create(MemphisId, TulaneId, UCFId, USFId),
                USFId.Create(SMUId, UCFId, CincyId, ECUId),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USM.Create(UAB, SMUId, RiceId, TulaneId),
                TulsaId.Create(USM, SMUId, RiceId, TempleId),
                UAB.Create(TulsaId, MemphisId, HoustonId, ECUId),
                SMUId.Create(UAB, MemphisId, HoustonId, CincyId),
                MemphisId.Create(USM, TulsaId, RiceId, TulaneId),
                RiceId.Create(UAB, SMUId, HoustonId, USFId),
                HoustonId.Create(USM, TulsaId, MemphisId, CharlotteId),
                TulaneId.Create(UCFId, CincyId, TempleId, USFId),
                UCFId.Create(TulsaId, RiceId, ECUId, TempleId),
                CincyId.Create(MemphisId, UCFId, TempleId, CharlotteId),
                ECUId.Create(SMUId, TulaneId, CincyId, CharlotteId),
                TempleId.Create(UAB, ECUId, CharlotteId, USFId),
                CharlotteId.Create(USM, TulaneId, UCFId, USFId),
                USFId.Create(HoustonId, UCFId, CincyId, ECUId),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateF()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USM.Create(UAB, SMUId, RiceId, TulaneId),
                TulsaId.Create(USM, SMUId, RiceId, CincyId),
                UAB.Create(TulsaId, MemphisId, HoustonId, TulaneId),
                SMUId.Create(UAB, MemphisId, HoustonId, USFId),
                MemphisId.Create(USM, TulsaId, RiceId, CharlotteId),
                RiceId.Create(UAB, SMUId, HoustonId, TempleId),
                HoustonId.Create(USM, TulsaId, MemphisId, ECUId),
                TulaneId.Create(UCFId, CincyId, TempleId, USFId),
                UCFId.Create(TulsaId, SMUId, ECUId, TempleId),
                CincyId.Create(UAB, UCFId, TempleId, CharlotteId),
                ECUId.Create(USM, TulaneId, CincyId, CharlotteId),
                TempleId.Create(HoustonId, ECUId, CharlotteId, USFId),
                CharlotteId.Create(RiceId, TulaneId, UCFId, USFId),
                USFId.Create(MemphisId, UCFId, CincyId, ECUId),
            }.Create();
        }

#else
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {USFId,new[] {UCFId,CharlotteId,SMUId,MemphisId} },
                {UCFId,new[] {ECUId,CincyId,TempleId,TulsaId} },
                {ECUId,new[] {USFId,TempleId,MemphisId,TulaneId} },
                {CharlotteId,new[] {UCFId,ECUId,CincyId,RiceId} },
                {CincyId,new[] {USFId,ECUId,TempleId,HoustonId} },
                {TempleId,new[] {USFId,CharlotteId,SMUId,TulaneId} },
                {TulsaId,new[] {CincyId,CharlotteId,RiceId,TulaneId} },
                {RiceId,new[] {UCFId,CincyId,SMUId,HoustonId} },
                {SMUId,new[] {ECUId,TulsaId,MemphisId,TulaneId} },
                {HoustonId,new[] {UCFId,CharlotteId,TulsaId,SMUId} },
                {MemphisId,new[] {TempleId,TulsaId,RiceId,HoustonId} },
                {TulaneId,new[] {USFId,RiceId,HoustonId,MemphisId} },
            };
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                {USFId,new[] {UCFId,CharlotteId,SMUId,MemphisId} },
                {UCFId,new[] {ECUId,CincyId,TempleId,TulsaId} },
                {ECUId,new[] {USFId,CincyId,TempleId,RiceId} },
                {CharlotteId,new[] {UCFId,ECUId,CincyId,TulaneId} },
                {CincyId,new[] {USFId,TempleId,MemphisId,TulaneId} },
                {TempleId,new[] {USFId,CharlotteId,RiceId,HoustonId} },
                {TulsaId,new[] {USFId,TempleId,RiceId,TulaneId} },
                {RiceId,new[] {CharlotteId,SMUId,HoustonId,MemphisId} },
                {SMUId,new[] {UCFId,CharlotteId,TulsaId,TulaneId} },
                {HoustonId,new[] {ECUId,CincyId,TulsaId,SMUId} },
                {MemphisId,new[] {UCFId,TulsaId,SMUId,HoustonId} },
                {TulaneId,new[] {ECUId,RiceId,HoustonId,MemphisId} },
            };
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new Dictionary<int, int[]>()
            {
                {USFId,new[] {UCFId,CharlotteId,RiceId,HoustonId} },
                {UCFId,new[] {ECUId,CincyId,TempleId,TulsaId} },
                {ECUId,new[] {USFId,CharlotteId,CincyId,SMUId} },
                {CharlotteId,new[] {UCFId,CincyId,TulsaId,MemphisId} },
                {CincyId,new[] {USFId,TempleId,RiceId,HoustonId} },
                {TempleId,new[] {USFId,ECUId,CharlotteId,TulaneId} },
                {TulsaId,new[] {ECUId,RiceId,MemphisId,TulaneId} },
                {RiceId,new[] {UCFId,SMUId,HoustonId,MemphisId} },
                {SMUId,new[] {CincyId,CharlotteId,TulsaId,TulaneId} },
                {HoustonId,new[] {TempleId,TulsaId,SMUId,TulaneId} },
                {MemphisId,new[] {ECUId,TempleId,SMUId,HoustonId} },
                {TulaneId,new[] {USFId,UCFId,RiceId,MemphisId} },
            };
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new Dictionary<int, int[]>()
            {
                {USFId,new[] {UCFId,CharlotteId,SMUId,MemphisId} },
                {UCFId,new[] {ECUId,CincyId,TempleId,TulsaId} },
                {ECUId,new[] {USFId,CharlotteId,TempleId,TulaneId} },
                {CharlotteId,new[] {UCFId,CincyId,HoustonId,TulaneId} },
                {CincyId,new[] {USFId,ECUId,TempleId,RiceId} },
                {TempleId,new[] {USFId,CharlotteId,TulsaId,RiceId} },
                {TulsaId,new[] {CincyId,RiceId,MemphisId,TulaneId} },
                {RiceId,new[] {ECUId,SMUId,HoustonId,MemphisId} },
                {SMUId,new[] {UCFId,CharlotteId,TulsaId,TulaneId} },
                {HoustonId,new[] {USFId,UCFId,TulsaId,SMUId} },
                {MemphisId,new[] {ECUId,CincyId,SMUId,HoustonId} },
                {TulaneId,new[] {TempleId,RiceId,HoustonId,MemphisId} },
            };
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new Dictionary<int, int[]>()
            {
                {USFId,new[] {UCFId,CharlotteId,RiceId,TulaneId} },
                {UCFId,new[] {ECUId,CincyId,TempleId,TulsaId} },
                {ECUId,new[] {USFId,CharlotteId,TempleId,HoustonId} },
                {CharlotteId,new[] {UCFId,CincyId,HoustonId,MemphisId} },
                {CincyId,new[] {USFId,ECUId,TempleId,TulaneId} },
                {TempleId,new[] {USFId,CharlotteId,SMUId,RiceId} },
                {TulsaId,new[] {USFId,ECUId,RiceId,MemphisId} },
                {RiceId,new[] {CharlotteId,SMUId,MemphisId,TulaneId} },
                {SMUId,new[] {ECUId,CincyId,TulsaId,TulaneId} },
                {HoustonId,new[] {TempleId,TulsaId,RiceId,SMUId} },
                {MemphisId,new[] {UCFId,CincyId,SMUId,HoustonId} },
                {TulaneId,new[] {UCFId,TulsaId,HoustonId,MemphisId} },
            };
        }
#endif
    }
}
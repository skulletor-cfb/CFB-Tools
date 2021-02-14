using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public    class AmericanSchedule
    {
        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateB, CreateC, CreateC, CreateD, CreateD, CreateE, CreateE, CreateA, CreateA, CreateB };
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
            var idx = (Form1.DynastyYear - 2211) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(12, RecruitingFixup.AmericanId, "American");
            AmericanConferenceSchedule = result.BuildHashSet();
            return result;
        }

        const int CincyId = 20;
        const int MemphisId = 48;
        const int SMUId = 83;
        const int UCONNId = 100;
        const int UCFId = 18;
        const int USFId = 144;
        const int ECUId = 25;
        const int TempleId = 90;
        const int TulsaId = 97;
        const int RiceId = 79;
        const int HoustonId = 33;
        const int TulaneId = 96;

        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {USFId,new[] {UCFId,UCONNId,SMUId,MemphisId} },
                {UCFId,new[] {ECUId,CincyId,TempleId,TulsaId} },
                {ECUId,new[] {USFId,TempleId,MemphisId,TulaneId} },
                {UCONNId,new[] {UCFId,ECUId,CincyId,RiceId} },
                {CincyId,new[] {USFId,ECUId,TempleId,HoustonId} },
                {TempleId,new[] {USFId,UCONNId,SMUId,TulaneId} },
                {TulsaId,new[] {CincyId,UCONNId,RiceId,TulaneId} },
                {RiceId,new[] {UCFId,CincyId,SMUId,HoustonId} },
                {SMUId,new[] {ECUId,TulsaId,MemphisId,TulaneId} },
                {HoustonId,new[] {UCFId,UCONNId,TulsaId,SMUId} },
                {MemphisId,new[] {TempleId,TulsaId,RiceId,HoustonId} },
                {TulaneId,new[] {USFId,RiceId,HoustonId,MemphisId} },
            };
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                {USFId,new[] {UCFId,UCONNId,SMUId,MemphisId} },
                {UCFId,new[] {ECUId,CincyId,TempleId,TulsaId} },
                {ECUId,new[] {USFId,CincyId,TempleId,RiceId} },
                {UCONNId,new[] {UCFId,ECUId,CincyId,TulaneId} },
                {CincyId,new[] {USFId,TempleId,MemphisId,TulaneId} },
                {TempleId,new[] {USFId,UCONNId,RiceId,HoustonId} },
                {TulsaId,new[] {USFId,TempleId,RiceId,TulaneId} },
                {RiceId,new[] {UCONNId,SMUId,HoustonId,MemphisId} },
                {SMUId,new[] {UCFId,UCONNId,TulsaId,TulaneId} },
                {HoustonId,new[] {ECUId,CincyId,TulsaId,SMUId} },
                {MemphisId,new[] {UCFId,TulsaId,SMUId,HoustonId} },
                {TulaneId,new[] {ECUId,RiceId,HoustonId,MemphisId} },
            };
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new Dictionary<int, int[]>()
            {
                {USFId,new[] {UCFId,UCONNId,RiceId,HoustonId} },
                {UCFId,new[] {ECUId,CincyId,TempleId,TulsaId} },
                {ECUId,new[] {USFId,UCONNId,CincyId,SMUId} },
                {UCONNId,new[] {UCFId,CincyId,TulsaId,MemphisId} },
                {CincyId,new[] {USFId,TempleId,RiceId,HoustonId} },
                {TempleId,new[] {USFId,ECUId,UCONNId,TulaneId} },
                {TulsaId,new[] {ECUId,RiceId,MemphisId,TulaneId} },
                {RiceId,new[] {UCFId,SMUId,HoustonId,MemphisId} },
                {SMUId,new[] {CincyId,UCONNId,TulsaId,TulaneId} },
                {HoustonId,new[] {TempleId,TulsaId,SMUId,TulaneId} },
                {MemphisId,new[] {ECUId,TempleId,SMUId,HoustonId} },
                {TulaneId,new[] {USFId,UCFId,RiceId,MemphisId} },
            };
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new Dictionary<int, int[]>()
            {
                {USFId,new[] {UCFId,UCONNId,SMUId,MemphisId} },
                {UCFId,new[] {ECUId,CincyId,TempleId,TulsaId} },
                {ECUId,new[] {USFId,UCONNId,TempleId,TulaneId} },
                {UCONNId,new[] {UCFId,CincyId,HoustonId,TulaneId} },
                {CincyId,new[] {USFId,ECUId,TempleId,RiceId} },
                {TempleId,new[] {USFId,UCONNId,TulsaId,RiceId} },
                {TulsaId,new[] {CincyId,RiceId,MemphisId,TulaneId} },
                {RiceId,new[] {ECUId,SMUId,HoustonId,MemphisId} },
                {SMUId,new[] {UCFId,UCONNId,TulsaId,TulaneId} },
                {HoustonId,new[] {USFId,UCFId,TulsaId,SMUId} },
                {MemphisId,new[] {ECUId,CincyId,SMUId,HoustonId} },
                {TulaneId,new[] {TempleId,RiceId,HoustonId,MemphisId} },
            };
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new Dictionary<int, int[]>()
            {
                {USFId,new[] {UCFId,UCONNId,RiceId,TulaneId} },
                {UCFId,new[] {ECUId,CincyId,TempleId,TulsaId} },
                {ECUId,new[] {USFId,UCONNId,TempleId,HoustonId} },
                {UCONNId,new[] {UCFId,CincyId,HoustonId,MemphisId} },
                {CincyId,new[] {USFId,ECUId,TempleId,TulaneId} },
                {TempleId,new[] {USFId,UCONNId,SMUId,RiceId} },
                {TulsaId,new[] {USFId,ECUId,RiceId,MemphisId} },
                {RiceId,new[] {UCONNId,SMUId,MemphisId,TulaneId} },
                {SMUId,new[] {ECUId,CincyId,TulsaId,TulaneId} },
                {HoustonId,new[] {TempleId,TulsaId,RiceId,SMUId} },
                {MemphisId,new[] {UCFId,CincyId,SMUId,HoustonId} },
                {TulaneId,new[] {UCFId,TulsaId,HoustonId,MemphisId} },
            };
        }
    }
}

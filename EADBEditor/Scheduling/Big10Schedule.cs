using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public static class Big10Schedule
    {
        const int Michigan = 51;
        const int Northwestern = 67;
        const int MichSt = 52;
        const int Iowa = 37;
        const int Minnesota = 54;
        const int Wisconsin = 114;
        const int OhioSt = 70;
        const int Illinois = 35;
        const int Indiana = 36;
        const int Purdue = 78;
        const int PennState = 76;
        const int Rutgers = 80;


        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] {
            CreateA, CreateB,
            CreateC, CreateA,
            CreateB, CreateC,
            CreateX, CreateY,
            CreateZ, CreateX,
            CreateY, CreateZ,
        };

        public static Dictionary<int, HashSet<int>> Big10ConferenceSchedule = null;
        public static Dictionary<int, int[]> ScenarioForSeason = null;

        public static void Init()
        {
            if (!initRun)
            {
                ScenarioForSeason = CreateScenarioForSeason();
                initRun = true;
            }
        }

        public static void ProcessBig10Schedule(Dictionary<int, TeamSchedule> schedule)
        {
            schedule.ProcessSchedule(ScenarioForSeason, Big10ConferenceSchedule, TableUtility.Big10Id, TableUtility.Big10);
        }

        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var idx = (Form1.DynastyYear - 2574) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(12, TableUtility.Big10Id, "Big10");
            Big10ConferenceSchedule = result.BuildHashSet();
            return result;
        }

        // no divisions
        public static Dictionary<int, int[]> CreateX()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(Michigan, Northwestern, Minnesota, Iowa),
                PennState.Create(Rutgers, OhioSt, Illinois, Iowa),
                OhioSt.Create(Rutgers, Michigan, Indiana, Wisconsin),
                Michigan.Create(MichSt, Indiana, Illinois, Minnesota),
                MichSt.Create(Rutgers, PennState, Purdue, Iowa),
                Indiana.Create(Rutgers, MichSt, Purdue, Wisconsin),
                Purdue.Create(PennState, OhioSt, Illinois, Minnesota),
                Illinois.Create(MichSt, Indiana, Northwestern, Wisconsin),
                Northwestern.Create(PennState, OhioSt, Michigan, Purdue),
                Wisconsin.Create(PennState, MichSt, Northwestern, Iowa),
                Minnesota.Create(OhioSt, Indiana, Northwestern, Wisconsin),
                Iowa.Create(Michigan, Purdue, Illinois, Minnesota),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateY()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(OhioSt, Indiana, Northwestern, Iowa),
                PennState.Create(Rutgers, OhioSt, Indiana, Wisconsin),
                OhioSt.Create(Michigan, Purdue, Illinois, Minnesota),
                Michigan.Create(PennState, MichSt, Indiana, Illinois),
                MichSt.Create(PennState, OhioSt, Northwestern, Wisconsin),
                Indiana.Create(MichSt, Purdue, Northwestern, Iowa),
                Purdue.Create(Rutgers, Michigan, Illinois, Wisconsin),
                Illinois.Create(Rutgers, MichSt, Northwestern, Minnesota),
                Northwestern.Create(PennState, Purdue, Minnesota, Iowa),
                Wisconsin.Create(Rutgers, Michigan, Illinois, Iowa),
                Minnesota.Create(PennState, MichSt, Indiana, Wisconsin),
                Iowa.Create(OhioSt, Michigan, Purdue, Minnesota),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateZ()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(Michigan, Purdue, Northwestern, Wisconsin),
                PennState.Create(Rutgers, OhioSt, Indiana, Illinois),
                OhioSt.Create(Michigan, Indiana, Illinois, Iowa),
                Michigan.Create(PennState, MichSt, Northwestern, Minnesota),
                MichSt.Create(Rutgers, OhioSt, Purdue, Minnesota),
                Indiana.Create(MichSt, Purdue, Northwestern, Iowa),
                Purdue.Create(PennState, Michigan, Illinois, Wisconsin),
                Illinois.Create(Rutgers, Indiana, Northwestern, Minnesota),
                Northwestern.Create(OhioSt, MichSt, Wisconsin, Iowa),
                Wisconsin.Create(OhioSt, Michigan, Indiana, Iowa),
                Minnesota.Create(Rutgers, PennState, Purdue, Wisconsin),
                Iowa.Create(PennState, MichSt, Illinois, Minnesota),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(Michigan, Indiana, Wisconsin, Iowa),
                PennState.Create(Rutgers, OhioSt, Purdue, Wisconsin),
                MichSt.Create(Rutgers, PennState, Illinois, Minnesota),
                Michigan.Create(PennState, MichSt, Indiana, Iowa),
                OhioSt.Create(Rutgers, Michigan, Illinois, Minnesota),
                Illinois.Create(PennState, Northwestern, Indiana, Iowa),
                Northwestern.Create(Rutgers, Michigan, OhioSt, Purdue),
                Purdue.Create(MichSt, OhioSt, Illinois, Wisconsin),
                Indiana.Create(MichSt, Northwestern, Purdue, Minnesota),
                Wisconsin.Create(Michigan, OhioSt, Northwestern, Iowa),
                Minnesota.Create(PennState, Illinois, Purdue, Wisconsin),
                Iowa.Create(MichSt, Northwestern, Indiana, Minnesota),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(Michigan, Illinois, Indiana , Iowa),
                PennState.Create(Rutgers, OhioSt, Indiana, Wisconsin),
                MichSt.Create(PennState, OhioSt, Illinois, Minnesota),
                Michigan.Create(MichSt, Northwestern, Minnesota, Iowa),
                OhioSt.Create(Rutgers, Michigan, Illinois, Indiana),
                Illinois.Create(Michigan, Northwestern, Wisconsin, Minnesota),
                Northwestern.Create(PennState, MichSt, Purdue, Iowa),
                Purdue.Create(Rutgers, PennState, Michigan, Illinois),
                Indiana.Create(MichSt, Northwestern, Purdue, Wisconsin),
                Wisconsin.Create(MichSt, OhioSt, Purdue, Iowa),
                Minnesota.Create(Rutgers, Northwestern, Indiana, Wisconsin),
                Iowa.Create(PennState, OhioSt, Purdue, Minnesota),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(MichSt, Illinois, Indiana, Wisconsin),
                PennState.Create(Rutgers, Michigan, Northwestern, Iowa),
                MichSt.Create(PennState, OhioSt, Wisconsin, Iowa),
                Michigan.Create(MichSt, Illinois, Purdue, Minnesota),
                OhioSt.Create(Michigan, Illinois, Indiana, Minnesota),
                Illinois.Create(PennState, Northwestern, Indiana, Wisconsin),
                Northwestern.Create(Rutgers, MichSt, OhioSt, Purdue),
                Purdue.Create(Rutgers, MichSt, OhioSt, Minnesota),
                Indiana.Create(PennState, Michigan, Purdue, Iowa),
                Wisconsin.Create(Michigan, Northwestern, Indiana, Iowa),
                Minnesota.Create(Rutgers, PennState, Northwestern, Wisconsin),
                Iowa.Create(OhioSt, Illinois, Purdue, Minnesota),
            }.Create();
        }
    }
}
using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public static class Big10Schedule
    {
        const int Michigan = 51;
        const int Northwestern = 67;
        const int MichiganState = 52;
        const int Iowa = 37;
        const int Minnesota = 54;
        const int Wisconsin = 114;
        const int OhioState = 70;
        const int Illinois = 35;
        const int Indiana = 36;
        const int Purdue = 78;
        const int PennState = 76;
        const int Rutgers = 80;
        const int Maryland = 47;
        const int Nebraska = 58;


        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] {
            CreateA, CreateB,
            CreateC, CreateD,
            CreateE, CreateA,
            CreateB, CreateC,
            CreateD, CreateE,
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
            var idx = (Form1.DynastyYear - 2587) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(14, TableUtility.Big10Id, "Big10");
            Big10ConferenceSchedule = result.BuildHashSet();
            return result;
        }

        // no divisions
        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
Rutgers.Create(Michigan, Iowa, Nebraska, Maryland),
PennState.Create(Rutgers, OhioState, Wisconsin, Iowa, Northwestern),
OhioState.Create(Rutgers, Michigan, Indiana, Northwestern),
Michigan.Create(MichiganState, Minnesota, Illinois, Nebraska, Maryland),
MichiganState.Create(PennState, Indiana, Minnesota, Northwestern),
Wisconsin.Create(Rutgers, Purdue, Iowa, Illinois, Nebraska),
Indiana.Create(Michigan, Wisconsin, Purdue, Northwestern),
Purdue.Create(OhioState, MichiganState, Minnesota, Illinois, Maryland),
Minnesota.Create(Rutgers, PennState, Wisconsin, Indiana),
Iowa.Create(Michigan, Minnesota, Northwestern, Nebraska, Maryland),
Northwestern.Create(Rutgers, Purdue, Illinois, Nebraska),
Illinois.Create(PennState, OhioState, MichiganState, Indiana, Iowa),
Nebraska.Create(OhioState, MichiganState, Purdue, Minnesota),
Maryland.Create(PennState, OhioState, MichiganState, Wisconsin, Indiana),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
Rutgers.Create(OhioState, MichiganState, Northwestern, Maryland),
PennState.Create(Rutgers, Michigan, Purdue, Iowa, Nebraska),
OhioState.Create(Michigan, Wisconsin, Minnesota, Maryland),
Michigan.Create(Rutgers, MichiganState, Indiana, Minnesota, Northwestern),
MichiganState.Create(PennState, OhioState, Wisconsin, Indiana),
Wisconsin.Create(Michigan, Purdue, Iowa, Northwestern, Maryland),
Indiana.Create(Rutgers, PennState, Purdue, Iowa),
Purdue.Create(Rutgers, Michigan, Iowa, Illinois, Nebraska),
Minnesota.Create(PennState, MichiganState, Wisconsin, Illinois),
Iowa.Create(OhioState, MichiganState, Minnesota, Northwestern, Nebraska),
Northwestern.Create(Purdue, Minnesota, Illinois, Nebraska),
Illinois.Create(Rutgers, OhioState, MichiganState, Indiana, Maryland),
Nebraska.Create(OhioState, Wisconsin, Indiana, Illinois),
Maryland.Create(PennState, Indiana, Minnesota, Northwestern, Nebraska),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
Rutgers.Create(MichiganState, Wisconsin, Nebraska, Maryland),
PennState.Create(Rutgers, OhioState, Wisconsin, Indiana, Illinois),
OhioState.Create(Rutgers, Michigan, Indiana, Northwestern),
Michigan.Create(PennState, MichiganState, Purdue, Minnesota, Iowa),
MichiganState.Create(PennState, Indiana, Nebraska, Maryland),
Wisconsin.Create(OhioState, Indiana, Iowa, Northwestern, Nebraska),
Indiana.Create(Purdue, Northwestern, Nebraska, Maryland),
Purdue.Create(OhioState, MichiganState, Minnesota, Iowa, Illinois),
Minnesota.Create(Rutgers, OhioState, Wisconsin, Indiana),
Iowa.Create(Rutgers, MichiganState, Minnesota, Illinois, Nebraska),
Northwestern.Create(PennState, MichiganState, Purdue, Illinois),
Illinois.Create(Rutgers, OhioState, Michigan, Wisconsin, Minnesota),
Nebraska.Create(Michigan, Minnesota, Northwestern, Maryland),
Maryland.Create(PennState, Michigan, Purdue, Iowa, Northwestern),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
Rutgers.Create(OhioState, Purdue, Minnesota, Maryland),
PennState.Create(Rutgers, OhioState, Indiana, Illinois, Nebraska),
OhioState.Create(Michigan, MichiganState, Minnesota, Iowa),
Michigan.Create(MichiganState, Purdue, Minnesota, Northwestern, Maryland),
MichiganState.Create(PennState, Wisconsin, Indiana, Iowa),
Wisconsin.Create(PennState, Michigan, Indiana, Iowa, Northwestern),
Indiana.Create(Rutgers, OhioState, Purdue, Northwestern),
Purdue.Create(PennState, MichiganState, Iowa, Illinois, Maryland),
Minnesota.Create(Wisconsin, Northwestern, Nebraska, Maryland),
Iowa.Create(Rutgers, Michigan, Indiana, Minnesota, Nebraska),
Northwestern.Create(OhioState, MichiganState, Purdue, Illinois),
Illinois.Create(Rutgers, OhioState, Wisconsin, Minnesota, Nebraska),
Nebraska.Create(Rutgers, Michigan, Wisconsin, Northwestern),
Maryland.Create(PennState, MichiganState, Indiana, Illinois, Nebraska),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
Rutgers.Create(Michigan, Indiana, Northwestern, Maryland),
PennState.Create(Rutgers, Purdue, Iowa, Northwestern, Nebraska),
OhioState.Create(Rutgers, Michigan, Purdue, Maryland),
Michigan.Create(PennState, MichiganState, Wisconsin, Minnesota, Illinois),
MichiganState.Create(Rutgers, PennState, OhioState, Indiana),
Wisconsin.Create(Rutgers, OhioState, MichiganState, Iowa, Nebraska),
Indiana.Create(Michigan, Purdue, Minnesota, Maryland),
Purdue.Create(Rutgers, Wisconsin, Minnesota, Illinois, Nebraska),
Minnesota.Create(PennState, MichiganState, Wisconsin, Northwestern),
Iowa.Create(OhioState, Indiana, Minnesota, Illinois, Nebraska),
Northwestern.Create(Michigan, Purdue, Iowa, Illinois),
Illinois.Create(OhioState, MichiganState, Indiana, Nebraska, Maryland),
Nebraska.Create(OhioState, MichiganState, Indiana, Northwestern),
Maryland.Create(PennState, Wisconsin, Minnesota, Iowa, Northwestern),
            }.Create();
        }

    }
}
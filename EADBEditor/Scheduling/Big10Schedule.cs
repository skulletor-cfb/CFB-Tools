using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        const int OSU = 70;
        const int Illinois = 35;
        const int Indiana = 36;
        const int Purdue = 78;
        const int PSU = 76;
        const int RU = 80;


        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] {
            CreateB, CreateB,
            CreateE, CreateE,
            CreateA, CreateA,
            CreateD, CreateD,
            CreateC, CreateC,
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
            schedule.ProcessSchedule(ScenarioForSeason, Big10ConferenceSchedule, RecruitingFixup.Big10Id, RecruitingFixup.Big10);
        }

        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var idx = (Form1.DynastyYear - 2581) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(12, RecruitingFixup.Big10Id, "Big10");
            Big10ConferenceSchedule = result.BuildHashSet();
            return result;
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                RU.Create(Michigan, Minnesota, Iowa,Northwestern),
                PSU.Create(RU, OSU, Michigan, Wisconsin, Illinois),
                OSU.Create(RU, Michigan, Purdue, Illinois),
                Michigan.Create(MichSt, Wisconsin, Indiana, Northwestern),
                MichSt.Create(RU, PSU, OSU, Wisconsin, Purdue),
                Wisconsin.Create(RU, OSU, Indiana, Iowa), 
                Indiana.Create(OSU, MichSt, Iowa, Northwestern),
                Purdue.Create(Michigan, Wisconsin, Indiana, Minnesota, Illinois),
                Minnesota.Create(PSU, MichSt, Wisconsin, Indiana, Northwestern),
                Iowa.Create(PSU, MichSt, Purdue, Minnesota, Illinois),
                Northwestern.Create(PSU, OSU, Purdue, Iowa),
                Illinois.Create(RU, Michigan, Indiana, Minnesota, Northwestern),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                RU.Create(OSU, MichSt, Purdue, Northwestern),
                PSU.Create(RU, OSU, Wisconsin, Indiana, Illinois),
                OSU.Create(Michigan, Wisconsin, Purdue, Iowa),
                Michigan.Create(RU, PSU, MichSt, Indiana, Minnesota),
                MichSt.Create(PSU, OSU, Wisconsin, Iowa, Northwestern),
                Wisconsin.Create(RU, Michigan, Iowa, Illinois),
                Indiana.Create(RU, OSU, Minnesota, Iowa, Northwestern),
                Purdue.Create(PSU, Michigan, Indiana, Illinois),
                Minnesota.Create(OSU, MichSt, Wisconsin, Purdue, Illinois),
                Iowa.Create(Michigan, Purdue, Minnesota, Northwestern, Illinois),
                Northwestern.Create(PSU, Wisconsin, Purdue, Minnesota),
                Illinois.Create(RU, MichSt, Indiana, Northwestern),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                RU.Create(Michigan, Purdue, Minnesota, Illinois),
                PSU.Create(RU, OSU, Purdue, Iowa, Northwestern),
                OSU.Create(RU, Michigan, Wisconsin, Illinois),
                Michigan.Create(PSU, MichSt, Minnesota, Iowa, Northwestern),
                MichSt.Create(OSU, Wisconsin, Purdue, Illinois),
                Wisconsin.Create(Michigan, Indiana, Iowa, Northwestern),
                Indiana.Create(RU, PSU, MichSt, Iowa, Northwestern),
                Purdue.Create(Wisconsin, Indiana, Minnesota, Illinois),
                Minnesota.Create(PSU, OSU, MichSt, Wisconsin, Indiana),
                Iowa.Create(RU, OSU, MichSt, Purdue, Minnesota),
                Northwestern.Create(RU, OSU, MichSt, Purdue),
                Illinois.Create(PSU, Michigan, Wisconsin, Indiana, Northwestern),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                RU.Create(),
                PSU.Create(),
                OSU.Create(),
                Michigan.Create(),
                MichSt.Create(),
                Wisconsin.Create(),
                Indiana.Create(),
                Purdue.Create(),
                Minnesota.Create(),
                Iowa.Create(),
                Northwestern.Create(),
                Illinois.Create(),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                RU.Create(),
                PSU.Create(),
                OSU.Create(),
                Michigan.Create(),
                MichSt.Create(),
                Wisconsin.Create(),
                Indiana.Create(),
                Purdue.Create(),
                Minnesota.Create(),
                Iowa.Create(),
                Northwestern.Create(),
                Illinois.Create(),
            }.Create();
        }
    }
}
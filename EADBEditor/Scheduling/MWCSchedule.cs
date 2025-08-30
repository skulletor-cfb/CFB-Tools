using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EA_DB_Editor
{
    public class MWCSchedule
    {
        private const int UNM = 60;
        private const int SDSU = 81;
        private const int Hawaii = 32;
        const int BSU = 12;
        const int SJSU = 82;
        const int FS = 29;
        const int Nevada = 59;
        const int UNLV = 101;
        const int AF = 1;
        const int Wyoming = 115;
        const int CSU = 23;
        const int UtahSt = 104;


        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { 
            CreateA, CreateA, 
            CreateB, CreateB,
            CreateC, CreateC,
            CreateD, CreateD,
        };
        public static Dictionary<int, HashSet<int>> MWCConferenceSchedule = null;
        public static Dictionary<int, int[]> ScenarioForSeason = null;

        public static void Init()
        {
            if (!initRun)
            {
                ScenarioForSeason = CreateScenarioForSeason();
                initRun = true;
            }
        }

        public static void ProcessMWCSchedule(Dictionary<int, TeamSchedule> schedule)
        {
            schedule.ProcessSchedule(ScenarioForSeason, MWCConferenceSchedule, RecruitingFixup.MWCId, RecruitingFixup.MWC);
        }



        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var idx = (Form1.DynastyYear - 2549) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(11, RecruitingFixup.MWCId, "MWC");
            MWCConferenceSchedule = result.BuildHashSet();
            return result;
        }

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wyoming.Create(UNLV, UtahSt, SJSU, SDSU),
                CSU.Create(Wyoming, Nevada, FS, SDSU),
                AF.Create(CSU, UNM, UtahSt, SJSU),
                Hawaii.Create(Wyoming, AF, Nevada, SJSU),
                UNLV.Create(CSU, Hawaii, UNM, FS),
                Nevada.Create(Wyoming, AF, UNLV, UtahSt),
                UNM.Create(CSU, Hawaii, Nevada, FS),
                UtahSt.Create(Hawaii, UNLV, UNM, SDSU),
                FS.Create(Wyoming, AF, UtahSt, SDSU),
                SJSU.Create(CSU, Nevada, UNM, FS ),
                SDSU.Create(AF, Hawaii, UNLV, SJSU),
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
    }
}
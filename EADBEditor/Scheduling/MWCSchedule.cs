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
        const int UTEP = 105;
        const int TexSt = 218;


        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] {
            CreateB, CreateB,
            CreateA, CreateA,
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
            var idx = (Form1.DynastyYear - 2561) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(14, RecruitingFixup.MWCId, "MWC");
            MWCConferenceSchedule = result.BuildHashSet();
            return result;
        }

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wyoming.Create(Nevada, UtahSt, SJSU, UTEP),
                CSU.Create(Wyoming, Hawaii, UNM, TexSt),
                AF.Create(CSU, UNLV, UtahSt, SDSU),
                Hawaii.Create(Wyoming, AF, UNLV, UTEP),
                BSU.Create(AF, Hawaii, UNLV, FS),
                Nevada.Create(CSU, BSU, UNM, SJSU),
                UNLV.Create(Nevada, FS, UTEP, TexSt),
                UNM.Create(Wyoming, BSU, SDSU, UTEP),
                UtahSt.Create(CSU, Hawaii, Nevada, SDSU),
                FS.Create(Wyoming, AF, Hawaii, SDSU),
                SJSU.Create(CSU, UNM, UtahSt, FS),
                SDSU.Create(BSU,UNLV, SJSU, TexSt),
                UTEP.Create(Nevada, UtahSt, SJSU, TexSt),
                TexSt.Create(AF, BSU, UNM, FS),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wyoming.Create(AF, UtahSt, SDSU, TexSt),
                CSU.Create(Wyoming, BSU, UNLV, FS),
                AF.Create(CSU, Nevada, SJSU, UTEP),
                Hawaii.Create(Wyoming, AF, SJSU, TexSt),
                BSU.Create(Wyoming, Hawaii, UtahSt, FS),
                Nevada.Create(Hawaii, BSU, UNM, SDSU),
                UNLV.Create(Wyoming, Nevada, UtahSt, UTEP),
                UNM.Create(AF, Hawaii, UNLV, UTEP),
                UtahSt.Create(CSU, UNM, FS, TexSt),
                FS.Create(Nevada, UNM, SDSU, UTEP),
                SJSU.Create(BSU, UNLV, UtahSt, FS),
                SDSU.Create(CSU, Hawaii, UNLV, SJSU),
                UTEP.Create(CSU, BSU, SDSU, TexSt),
                TexSt.Create(AF, Nevada, UNM, SJSU),
            }.Create();
        }
    }
}
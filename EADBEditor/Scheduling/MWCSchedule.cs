using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EA_DB_Editor
{
    public class MWCSchedule
    {
        private const int UTEPId = 105;
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
            CreateA, CreateB, 
            CreateC, CreateA,
            CreateB, CreateC,
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
            if (Form1.DynastyYear == 2524)
            {
                throw new Exception("Does BSU go to Big 12????");
            }

            var idx = (Form1.DynastyYear - 2478) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(12, RecruitingFixup.MWCId, "MWC");
            MWCConferenceSchedule = result.BuildHashSet();
            return result;
        }

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wyoming.Create(AF, Nevada, UtahSt, SJSU),
                CSU.Create(Wyoming, Hawaii, UNM, SDSU),
                AF.Create(CSU, UNLV, UtahSt, FS),
                Hawaii.Create(Wyoming, AF, Nevada, SDSU),
                BSU.Create(CSU, Hawaii, UNLV, FS),
                Nevada.Create(AF, BSU, UNM, SJSU),
                UNLV.Create(CSU, Nevada, UtahSt, SDSU),
                UNM.Create(Wyoming, BSU, UNLV, FS),
                UtahSt.Create(Hawaii, BSU, UNM, SJSU),
                FS.Create(CSU, Nevada, UtahSt, SDSU),
                SJSU.Create(AF, Hawaii, UNLV, FS),
                SDSU.Create(Wyoming, BSU, UNM, SJSU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wyoming.Create(AF, Nevada, UtahSt, FS),
                CSU.Create(Wyoming, BSU, UtahSt, FS),
                AF.Create(CSU, UNLV, UNM, SJSU),
                Hawaii.Create(AF, UNLV, UNM, SDSU),
                BSU.Create(Wyoming, AF, Hawaii, FS),
                Nevada.Create(CSU,  Hawaii, BSU, SDSU),
                UNLV.Create(Wyoming, Nevada, UtahSt, SDSU),
                UNM.Create(CSU, BSU, UNLV, SJSU),
                UtahSt.Create(Hawaii, Nevada, UNM, SJSU),
                FS.Create(Hawaii, UNLV, UNM, SDSU),
                SJSU.Create(CSU, BSU,  Nevada, FS),
                SDSU.Create(Wyoming, AF, UtahSt, SJSU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Wyoming.Create(BSU, UNLV, UtahSt, SJSU),
                CSU.Create(Wyoming, Hawaii, Nevada,SDSU),
                AF.Create(CSU, UNM, UtahSt, FS),
                Hawaii.Create(Wyoming, AF, UNLV, SJSU),
                BSU.Create(AF, Hawaii, UtahSt, SDSU),
                Nevada.Create(AF, BSU, UNM, FS),
                UNLV.Create(CSU, BSU, Nevada, SJSU),
                UNM.Create(Wyoming, Hawaii, UNLV, SDSU),
                UtahSt.Create(CSU, Nevada, UNM, FS),
                FS.Create(Wyoming, Hawaii, UNLV, SDSU),
                SJSU.Create(CSU, BSU, UNM, FS),
                SDSU.Create(AF, Nevada, UtahSt, SJSU),
            }.Create();
        }



        /*
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {Hawaii,new[] {SDSU,UNLV,Nevada,CSU } },
                {SDSU,new[] {SJSU,UNLV,Wyoming,UTEPId } },
                {FS,new[] { Hawaii, SDSU,UTEPId,UtahSt } },
                {SJSU,new[] { Hawaii, FS,Nevada,UNM } },
                {UNLV,new[] {FS,SJSU,Nevada,AF } },
                {Nevada,new[] {SDSU,FS,Wyoming,UtahSt } },
                {CSU,new[] { SJSU,UNLV,Wyoming,AF} },
                {Wyoming,new[] {FS,UNM,UTEPId,UtahSt } },
                {UNM,new[] { Hawaii, UNLV,CSU,AF } },
                {UTEPId,new[] {Nevada,CSU,UNM,AF } },
                {UtahSt,new[] {SDSU,CSU,UNM,UTEPId } },
                {AF,new[] { Hawaii, SJSU,Wyoming,UtahSt } },
            };
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                {Hawaii,new[] {SDSU,UNLV,Nevada,Wyoming } },
                {SDSU,new[] {SJSU,Nevada,CSU,UNM } },
                {FS,new[] { Hawaii, SDSU,UNM,AF } },
                {SJSU,new[] { Hawaii, FS,UNLV,UTEPId } },
                {UNLV,new[] {SDSU,FS,Nevada,UtahSt } },
                {Nevada,new[] {FS,SJSU,CSU,AF} },
                {CSU,new[] {FS,Wyoming,UTEPId,AF } },
                {Wyoming,new[] {SJSU,UNLV,UTEPId,UtahSt} },
                {UNM,new[] {Nevada,CSU,Wyoming,AF } },
                {UTEPId,new[] { Hawaii, UNLV,UNM,UtahSt} },
                {UtahSt,new[] { Hawaii, SJSU,CSU,UNM } },
                {AF,new[] {SDSU,Wyoming,UTEPId,UtahSt } },
            };
        }*/
    }
}
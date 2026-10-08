using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public static class ACCPodSchedule
    {
        private static bool initRun = false;
        public static Dictionary<int, HashSet<int>> ACCConferenceSchedule = null;
        public static Dictionary<int, int[]> ScenarioForSeason = null;
        public const int Miami = 49;
        public const int BC = 13;
        public const int VT = 108;
        public const int UVA = 107;
        public const int SU = 88;
        public const int Pitt = 77;
        public const int WVU = 112;
        public const int UNC = 62;
        public const int Duke = 24;
        public const int WF = 109;
        public const int NCSU = 63;
        public const int FSU = 28;
        public const int Clemson = 21;
        public const int UL = 44;
        public const int GT = 31;

        public static void Init()
        {
            if (!initRun)
            {
                ScenarioForSeason = CreateScenarioForSeason();
                initRun = true;
            }
        }

        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] {
            CreateA, CreateA,
            CreateB, CreateB,
        };

        public static void ProcessACCSchedule(Dictionary<int, TeamSchedule> schedule)
        {
            schedule.ProcessSchedule(
                ScenarioForSeason,
                ACCConferenceSchedule,
                TableUtility.ACCId,
                TableUtility.ACC,
                68);
        }


        private static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            Dictionary<int, int[]> result = null;
            var idx = (Form1.DynastyYear - 2487) % Creators.Length;
            result = Creators[idx]();
            var dict = result.Verify(15, TableUtility.ACCId, "ACC");
            ACCConferenceSchedule = dict.BuildHashSet();
            return dict;
        }

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
Miami.Create(FSU, Duke, UVA, SU),
FSU.Create(Clemson, UNC, VT, BC),
Clemson.Create(NCSU, UVA, WVU, Pitt),
GT.Create(Miami, Clemson, UNC, BC),
UL.Create(FSU, GT, WF, WVU),
Duke.Create(Clemson, UL, UNC, VT),
WF.Create(FSU, GT, Duke, WVU),
NCSU.Create(Miami, WF, SU, BC),
UNC.Create(UL, NCSU, UVA, Pitt),
UVA.Create(UL, WF, NCSU, VT),
VT.Create(GT, UNC, Pitt, BC),
WVU.Create(Miami, NCSU, VT, Pitt),
Pitt.Create(FSU, UL, Duke, SU),
SU.Create(Clemson, GT, WF, UVA),
BC.Create(Miami, Duke, WVU, SU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            // redo home/away for ACC rivalries
            return new List<KeyValuePair<int, int[]>>
            {
Miami.Create(FSU, UL, WF, Pitt),
FSU.Create(Clemson, NCSU, UVA, WVU),
Clemson.Create(Miami, UNC, VT, BC),
GT.Create(FSU, Clemson, NCSU, Pitt),
UL.Create(Clemson, GT, NCSU, BC),
Duke.Create(FSU, GT, UL, WVU),
WF.Create(Clemson, Duke, UNC, BC),
NCSU.Create(Duke, WF, VT, Pitt),
UNC.Create(Miami, NCSU, UVA, SU),
UVA.Create(GT, Duke, VT, WVU),
VT.Create(Miami, UL, WF, SU),
WVU.Create(GT, UNC, VT, Pitt),
Pitt.Create(WF, UVA, SU, BC),
SU.Create(FSU, UL, Duke, WVU),
BC.Create(Miami, UNC, UVA, SU),
            }.Create();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public const int UMD = 47;
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
            if (RecruitingFixup.ACC.Length < 16)
                return;

            if(RecruitingFixup.ACCConfTeams.Length==14)
            {
                // todo if acc is 14 games fix something
            }

            if (!initRun)
            {
                ScenarioForSeason = CreateScenarioForSeason();
                initRun = true;
            }
        }

        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] {
            CreateC, CreateA,
            CreateB, CreateC,
            CreateA, CreateB,
        };

        public static void ProcessACCSchedule(Dictionary<int, PreseasonScheduledGame[]> schedule)
        {
            schedule.ProcessSchedule(
                ScenarioForSeason,
                ACCConferenceSchedule,
                RecruitingFixup.ACCId,
                RecruitingFixup.ACC,
                68);
        }


        private static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            Dictionary<int, int[]> result = null;
#if false
            if (RecruitingFixup.TeamAndDivision[49] == RecruitingFixup.TeamAndDivision[24])
                result = CreateA();
            else if (RecruitingFixup.TeamAndDivision[49] == RecruitingFixup.TeamAndDivision[31])
                result = CreateB();
            else if (RecruitingFixup.TeamAndDivision[49] == RecruitingFixup.TeamAndDivision[112])
                result = CreateC();
            else
                throw new Exception("THIS SHOULDNT HAPPEN");
#else
            var idx = (Form1.DynastyYear - 2487) % Creators.Length;
            result = Creators[idx]();
#endif
            var dict = result.Verify(16, RecruitingFixup.ACCId, "ACC");

            ACCConferenceSchedule = dict.BuildHashSet();
            return dict;
        }

#if false // the old acc pods
        public static Dictionary<int, int[]> CreateA()
        {
            var dict= new Dictionary<int, int[]>();
            dict.Add(UMD, SU, WVU, Pitt, FSU);
            dict.Add(SU, WVU, UL, GT, BC);
            dict.Add(WVU, Pitt, Clemson, UL, GT);
            dict.Add(Pitt, SU, Clemson, GT, WF);
            dict.Add(FSU, SU, WVU, Pitt, Clemson);
            dict.Add(Clemson, UMD, SU, UL, NCSU);
            dict.Add(UL, UMD, Pitt, FSU, GT);
            dict.Add(GT, UMD, UNC, FSU, Clemson);
            dict.Add(UVA, NCSU, Duke, VT, UMD);
            dict.Add(BC, UVA, UNC, Miami, VT);
            dict.Add(UNC, UVA, Miami, NCSU, VT);
            dict.Add(WF, BC, UNC, Duke, UVA);
            dict.Add(Miami, UVA, WF, NCSU, FSU);
            dict.Add(NCSU, BC, WF, Duke, VT);
            dict.Add(Duke, BC, UNC, Miami, UL);
            dict.Add(VT, WF, Miami, Duke, WVU);
            return dict;
        }

        public static Dictionary<int, int[]> CreateB()
        {
            var dict = new Dictionary<int, int[]>();
            dict.Add(UMD, SU, WVU, NCSU, FSU);
            dict.Add(SU, WVU, NCSU, WF, BC);
            dict.Add(WVU, Pitt, UNC, WF, VT);
            dict.Add(Pitt, UMD, SU, UNC, Duke);
            dict.Add(UNC, UMD, SU, NCSU, UVA);
            dict.Add(NCSU, WVU, Pitt, WF, Duke);
            dict.Add(WF, UMD, Pitt, UNC, Duke);
            dict.Add(Duke, UMD, SU, WVU, UNC);

            dict.Add(FSU, BC, VT, Clemson, UL);
            dict.Add(BC, VT, Miami, UVA, GT);
            dict.Add(VT, Miami, Clemson, GT, UL);
            dict.Add(Miami, FSU, UVA, GT, Pitt);
            dict.Add(UVA, FSU, VT, Clemson, UL);
            dict.Add(Clemson, BC, Miami, UL, NCSU);
            dict.Add(GT, FSU, UVA, Clemson, WF);
            dict.Add(UL, BC, Miami, GT, Duke);

            return dict;
        }

        public static Dictionary<int, int[]> CreateC()
        {
            var dict = new Dictionary<int, int[]>();

            dict.Add(FSU, Clemson, UL, NCSU, Duke);
            dict.Add(Clemson, UL, NCSU, WF, BC);
            dict.Add(UL, GT, UNC,WF, WVU);
            dict.Add(GT, FSU, Clemson, UNC, WF);
            dict.Add(UNC, FSU, Clemson, NCSU, UVA);
            dict.Add(WF, FSU, UNC, Duke, SU);
            dict.Add(NCSU, UL, GT, WF, Duke);
            dict.Add(Duke, Clemson, UL, GT, UNC);

            dict.Add(Miami, UVA, UMD, Pitt, FSU);
            dict.Add(BC, Miami, VT, UVA, Pitt);
            dict.Add(WVU, Miami, BC, VT, Pitt);
            dict.Add(VT, Miami, UMD, SU, GT);

            dict.Add(UVA, WVU, VT, UMD, Pitt);
            dict.Add(UMD, BC, WVU, SU, NCSU);
            dict.Add(SU, Miami, BC, WVU, UVA);
            dict.Add(Pitt, VT, UMD, SU, Duke);

            return dict;
        }
#else
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Miami.Create(UVA, Duke, FSU, Pitt),
                BC.Create(Miami, SU, GT, WF),
                WVU.Create(BC, VT, Pitt, UL),
                VT.Create(Miami, BC, FSU, Pitt),
                Clemson.Create(WVU, VT, NCSU, UL),
                SU.Create(Miami, WVU, NCSU, UMD),
                UVA.Create(VT, SU, Duke, UMD),
                Duke.Create(BC, UNC, UMD, Pitt),
                NCSU.Create(WVU, VT, Duke, WF),
                UNC.Create(Clemson, UVA, NCSU, UL),
                FSU.Create(BC, Clemson, UVA, NCSU),
                UMD.Create(Miami, WVU, Clemson, UNC),
                Pitt.Create(SU, GT, WF, UL),
                GT.Create(Clemson, UVA, UNC, FSU),
                WF.Create(Duke, UNC, UMD, GT),
                UL.Create(SU, FSU, GT, WF),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            // redo home/away for ACC rivalries
            return new List<KeyValuePair<int, int[]>>
            {
                Miami.Create(WVU, UNC, FSU, Pitt),
                BC.Create(Miami, SU, UVA, UMD),
                WVU.Create(VT, Duke, Pitt, UL),
                VT.Create(Miami, BC, GT, WF),
                Clemson.Create(Miami, BC, NCSU, UL),
                SU.Create(WVU, UNC, FSU, GT),
                UVA.Create(VT,Clemson, NCSU, UMD),
                Duke.Create(SU, UNC, UMD, UL),
                NCSU.Create(Duke, Pitt, GT, WF),
                UNC.Create(BC, WVU, UVA, NCSU),
                FSU.Create(Clemson, UVA, Duke, GT),
                UMD.Create(WVU, VT, NCSU, FSU),
                Pitt.Create(BC, SU, UMD, UL),
                GT.Create(Clemson, UVA, Pitt, WF),
                WF.Create(Clemson, Duke, UNC, FSU),
                UL.Create(Miami, VT, SU, WF),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Miami.Create(SU, FSU, GT, WF),
                BC.Create(Miami, SU, GT, UL),
                WVU.Create(VT, FSU, Pitt, WF),
                VT.Create(Miami, BC, UNC, GT),
                Clemson.Create(BC, SU, NCSU, Pitt),
                SU.Create(WVU, VT, UMD, WF),
                UVA.Create(WVU, VT, UMD, Pitt),
                Duke.Create(VT, Clemson, UNC, UL),
                NCSU.Create(Miami, BC, Duke, WF),
                UNC.Create(UVA, NCSU, UMD, Pitt),
                FSU.Create(Clemson, UVA, UNC, UL),
                UMD.Create(WVU, NCSU, FSU, UL),
                Pitt.Create(BC, SU, Duke, FSU),
                GT.Create(WVU, Clemson, Duke, UMD),
                WF.Create(Clemson, UVA, Duke, UNC),
                UL.Create(Miami, UVA, NCSU, GT),
            }.Create();
        }

#endif

        public static void Add(this Dictionary<int,int[]> dict,int key,params int[] values)
        {
            dict.Add(key, values);
        }
#if false
        public static Dictionary<int, int[]> CreateB()
        {
#if false
            return new Dictionary<int, int[]>()
            {
                {49,new[]{13,107,47,28}},
                {13,new[]{108,62,24,21}},
                {108,new[]{49,88,47,31}},
                {88,new[]{49,13,24,77}},
                {107,new[]{13,108,88,47}},
                {62,new[]{49,108,88,107}},
                {47,new[]{13,88,62,24}},
                {24,new[]{49,108,107,62}},
                {28,new[]{21,LouisvilleId,63,109}},
                {21,new[]{31,LouisvilleId,63,112}},
                {31,new[]{28,77,112,109}},
                {77,new[]{28,21,LouisvilleId,63}},
                {LouisvilleId,new[]{31,63,112,107}},
                {63,new[]{31,112,109,62}},
                {112,new[]{28,77,47,109}},
                {109,new[]{21,77,LouisvilleId,24}},
        };
#else
            return new Dictionary<int, int[]>()
            {
                {49,new[]{LouisvilleId,107,24,28}},
                {13,new[]{49,107,62,24}},
                {108,new[]{49,13,62,24}},
                {88,new[]{28,112,47,13}},
                {107,new[]{108,LouisvilleId,62,31}},
                {62,new[]{49,LouisvilleId,24,63}},
                {47,new[]{112,109,21,107}},
                {24,new[]{LouisvilleId,107,31,109}},
                {28,new[]{77,47,63,109}},
                {21,new[]{28,88,109,31}},
                {31,new[]{49,13,108,62}},
                {77,new[]{88,47,63,21}},
                {LouisvilleId,new[]{13,108,31,77}},
                {63,new[]{88,112,47,21}},
                {112,new[]{28,77,21,108}},
                {109,new[]{88,112,77,63}},
        };
#endif
        }

        public static Dictionary<int, int[]> CreateC()
        {
#if false
            return new Dictionary<int, int[]>()
            {
                {49,new[]{28,21,13,63}},
                {109,new[]{49,28,13,24}},
                {108,new[]{49,109,21,63}},
                {88,new[]{49,109,108,77}},
                {28,new[]{108,88,21,63}},
                {21,new[]{109,88,13,31}},
                {13,new[]{108,88,28,63}},
                {63,new[]{109,88,21,62}},
            
                {LouisvilleId,new[]{107,31,62,49}},
                {24,new[]{LouisvilleId,107,77,47}},
                {107,new[]{77,112,62,108}},
                {77,new[]{LouisvilleId,47,31,112}},
                {47,new[]{LouisvilleId,107,31,28}},
                {31,new[]{24,107,112,62}},
                {112,new[]{LouisvilleId,24,47,13}},
                {62,new[]{24,77,47,112}}
        };
#else
            return new Dictionary<int, int[]>()
            {
                {49,new[]{LouisvilleId,28,21,77}},
                {109,new[]{13,108,49,63}},
                {108,new[]{13,49,21,63}},
                {88,new[]{112,31,24,13}},
                {28,new[]{13,108,109,47}},
                {21,new[]{LouisvilleId,28,109,31}},
                {13,new[]{LouisvilleId,49,21,63}},
                {63,new[]{LouisvilleId,49,28,21}},

                {LouisvilleId,new[]{108,28,109,112}},
                {24,new[]{112,107,47,109}},
                {107,new[]{112,88,62,108}},
                {77,new[]{88,107,47,24}},
                {47,new[]{88,107,31,62}},
                {31,new[]{107,77,24,62}},
                {112,new[]{77,47,31,62}},
                {62,new[]{88,77,24,63}}
        };
#endif
        }


        public static Dictionary<int, int[]> CreateA()
        {
#if true
            return new Dictionary<int, int[]>()
            {
                {49,new[]{LouisvilleId,47,77,28}},
                {13,new[]{49,108,LouisvilleId,47}},
                {108,new[]{49,LouisvilleId,88,112}},
                {LouisvilleId,new[]{47,112,77,31}},

                {47,new[]{108,88,77,62}},
                {88,new[]{49,13,LouisvilleId,112}},
                {112,new[]{13,49,47,77}},
                {77,new[]{13,108,88,24}},

                {28,new[]{21,31,63,24}},
                {21,new[]{107,31,109,13}},
                {107,new[]{28,31,109,108}},
                {31,new[]{62,63,109,24}},

                {62,new[]{28,21,107,63}},
                {63,new[]{21,107,109,88}},
                {109,new[]{28,62,24,112}},
                {24,new[]{21,107,62,63}},
            };
#else
            return new Dictionary<int, int[]>()
            {
                {49,new[]{88,13,77,28}},
                {88,new[]{108,112,77,109}},
                {108,new[]{49,LouisvilleId,13,77}},
                {LouisvilleId,new[]{49,88,112,31}},
                {112,new[]{49,108,13,47}},
                {13,new[]{88,LouisvilleId,31,21}},
                {31,new[]{49,88,108,112}},
                {77,new[]{LouisvilleId,112,13,31}},

                {28,new[]{109,107,21,63}},
                {107,new[]{109,62,24,108}},
                {47,new[]{28,107,63,24}},
                {21,new[]{107,62,47,63}},

                {62,new[]{28,47,24,LouisvilleId}},
                {63,new[]{107,62,24,31}},
                {24,new[]{28,109,77,21}},
                {109,new[]{62,47,21,63}},
            };

            return new Dictionary<int, int[]>()
            {
                {49,new[]{88,31,77,28}},
                {77,new[]{108,112,88,109}},
                {108,new[]{49,LouisvilleId,31,88}},
                {LouisvilleId,new[]{49,77,112,13}},
                {112,new[]{49,108,31,47}},
                {31,new[]{77,LouisvilleId,13,21}},
                {13,new[]{49,77,108,112}},
                {88,new[]{LouisvilleId,112,31,13}},

                {28,new[]{109,107,21,63}},
                {107,new[]{109,62,24,108}},
                {47,new[]{28,107,63,24}},
                {21,new[]{107,62,47,63}},
                {62,new[]{28,47,24,LouisvilleId}},
                {63,new[]{107,62,24,13}},
                {24,new[]{28,109,88,21}},
                {109,new[]{62,47,21,63}},
            };
#endif
        }
#endif
    }
}
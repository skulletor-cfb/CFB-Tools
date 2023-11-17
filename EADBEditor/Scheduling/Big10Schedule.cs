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
        const int OhioSt = 70;
        const int Illinois = 35;
        const int Indiana = 36;
        const int Purdue = 78;
        const int PennState = 76;
        const int Rutgers = 80;


        private static bool initRun = false;
        // public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateG, CreateG, CreateA, CreateA, CreateH, CreateH, CreateD, CreateD, CreateJ, CreateJ, CreateB, CreateB, CreateI, CreateI, CreateE, CreateE, CreateF, CreateF, CreateC, CreateC, };
        //public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateA, CreateA, CreateB, CreateB, CreateC, CreateC, CreateD, CreateD, CreateE, CreateE };
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { 
            CreateB, CreateB,
            CreateY, CreateY,
            CreateC, CreateC, // no ohio st - penn st
            CreateZ, CreateZ,
            CreateA, CreateA,
            CreateX, CreateX, //no ohio st- ill
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

        public static void ProcessBig10Schedule(Dictionary<int, PreseasonScheduledGame[]> schedule)
        {
            schedule.ProcessSchedule(ScenarioForSeason, Big10ConferenceSchedule, RecruitingFixup.Big10Id, RecruitingFixup.Big10);
        }

        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            if (Form1.DynastyYear == 2488) throw new Exception("Reorder Big 10 creators: a-b-c-a-b-c-x-y-z-x-y-z");
            var idx = (Form1.DynastyYear - 2476) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(12, RecruitingFixup.Big10Id, "Big10");
            Big10ConferenceSchedule = result.BuildHashSet();
            return result;
        }

#if true // no divisions
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
#elif false // east-west
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                OhioSt.Create(Michigan, Indiana, Rutgers, Illinois),
                Michigan.Create(PennState, MichSt, Minnesota, Purdue),
                Indiana.Create(Michigan, MichSt, Rutgers, Purdue),
                PennState.Create(OhioSt, Indiana, Rutgers, Northwestern),
                MichSt.Create(OhioSt, PennState, Illinois, Wisconsin),
                Rutgers.Create(Michigan, MichSt, Iowa, Northwestern),

                Illinois.Create(PennState, Minnesota, Iowa, Northwestern),
                Minnesota.Create(OhioSt, MichSt, Northwestern, Wisconsin),
                Purdue.Create(OhioSt, Illinois, Minnesota, Wisconsin),
                Iowa.Create(Michigan, Indiana, Minnesota, Purdue),
                Northwestern.Create(Indiana, Purdue, Iowa, Wisconsin),
                Wisconsin.Create(PennState, Rutgers, Illinois, Iowa)
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return null;
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return null;
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return null;
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return null;
        }

#elif true // north-south

        public static Dictionary<int, int[]> CreateA()
        {
            var dict = new Dictionary<int, int[]>();
            dict.Create(Michigan, Northwestern, MichSt, Rutgers, PennState);
            dict.Create(Northwestern, MichSt, Iowa, Wisconsin, OhioSt);
            dict.Create(MichSt, Iowa, Minnesota, Wisconsin, OhioSt);
            dict.Create(Iowa, Michigan, Minnesota, Illinois, Purdue);
            dict.Create(Minnesota, Michigan, Northwestern, Indiana, PennState);
            dict.Create(Wisconsin, Michigan, Iowa, Minnesota, Purdue);

            dict.Create(OhioSt, Michigan, Illinois, Indiana, Rutgers);
            dict.Create(Illinois, Northwestern, MichSt, Indiana, PennState);
            dict.Create(Indiana, MichSt, Iowa, Purdue, Rutgers);
            dict.Create(Purdue, Minnesota, OhioSt, Illinois, Rutgers);
            dict.Create(PennState, Wisconsin, OhioSt, Indiana, Purdue);
            dict.Create(Rutgers, Northwestern, Wisconsin, Illinois, PennState);
            return dict;
        }

        public static Dictionary<int, int[]> CreateB()
        {
            var dict = new Dictionary<int, int[]>();
            dict.Create(Michigan, MichSt, Iowa, Wisconsin, Indiana);
            dict.Create(Northwestern, Michigan, Iowa, Wisconsin, Purdue);
            dict.Create(MichSt, Northwestern, Iowa, Minnesota, PennState);
            dict.Create(Iowa, Minnesota, OhioSt, Purdue, Rutgers);
            dict.Create(Minnesota, Michigan, Northwestern, OhioSt, PennState);
            dict.Create(Wisconsin, MichSt, Iowa, Minnesota, Rutgers);

            dict.Create(OhioSt, Michigan, Illinois, Indiana, Rutgers);
            dict.Create(Illinois, Northwestern, Minnesota, Wisconsin, Indiana);
            dict.Create(Indiana, MichSt, Wisconsin, Purdue, Rutgers);
            dict.Create(Purdue, Michigan, OhioSt, Illinois, PennState);
            dict.Create(PennState, Northwestern, OhioSt, Illinois, Indiana);
            dict.Create(Rutgers, MichSt, Illinois, Purdue, PennState);
            return dict;
        }

        public static Dictionary<int, int[]> CreateC()
        {
            var dict = new Dictionary<int, int[]>();
            dict.Create(Michigan, MichSt, Wisconsin, Illinois, Rutgers);
            dict.Create(Northwestern, Michigan, Iowa, Wisconsin, OhioSt);
            dict.Create(MichSt, Northwestern, Iowa, Wisconsin, Purdue);
            dict.Create(Iowa, Michigan, Minnesota, Indiana, Purdue);
            dict.Create(Minnesota, Michigan, Northwestern, MichSt, PennState);
            dict.Create(Wisconsin, Iowa, Minnesota, OhioSt, PennState);

            dict.Create(OhioSt, Michigan, Illinois, Indiana, Rutgers);
            dict.Create(Illinois, Northwestern, MichSt, Indiana, Rutgers);
            dict.Create(Indiana, Northwestern, MichSt, Purdue, Rutgers);
            dict.Create(Purdue, Minnesota, OhioSt, Illinois, PennState);
            dict.Create(PennState, Iowa, OhioSt, Illinois, Indiana);
            dict.Create(Rutgers, Minnesota, Wisconsin, Purdue, PennState);

            return dict;
        }

        public static Dictionary<int, int[]> CreateD()
        {
            var dict = new Dictionary<int, int[]>();
            dict.Create(Michigan, Northwestern, MichSt, Purdue, PennState);
            dict.Create(Northwestern, MichSt, Iowa, Wisconsin, Rutgers);
            dict.Create(MichSt, Iowa, Minnesota, Wisconsin, OhioSt);
            dict.Create(Iowa, Michigan, Minnesota, OhioSt, Purdue);
            dict.Create(Minnesota, Michigan, Northwestern, PennState, Illinois);
            dict.Create(Wisconsin, Michigan, Iowa, Minnesota, Indiana);

            dict.Create(OhioSt, Michigan, Illinois, Indiana, Rutgers);
            dict.Create(Illinois, Northwestern, Iowa, Indiana, PennState);
            dict.Create(Indiana, MichSt, Minnesota, Purdue, Rutgers);
            dict.Create(Purdue, Wisconsin, OhioSt, Illinois, Rutgers);
            dict.Create(PennState, Northwestern, OhioSt, Indiana, Purdue);
            dict.Create(Rutgers, MichSt, Wisconsin, Illinois, PennState);
            return dict;
        }

        public static Dictionary<int, int[]> CreateE()
        {
            var dict = new Dictionary<int, int[]>();
            dict.Create(Michigan, Northwestern, MichSt, Illinois, Indiana);
            dict.Create(Northwestern, MichSt, Iowa, Wisconsin, Purdue);
            dict.Create(MichSt, Iowa, Minnesota, Wisconsin, PennState);
            dict.Create(Iowa, Michigan, Minnesota, Purdue, Rutgers);
            dict.Create(Minnesota, Michigan, Northwestern, OhioSt, PennState);
            dict.Create(Wisconsin, Michigan, Iowa, Minnesota, OhioSt);

            dict.Create(OhioSt, Michigan, Illinois, Indiana, Rutgers);
            dict.Create(Illinois, Northwestern, Wisconsin, Indiana, PennState);
            dict.Create(Indiana, Northwestern, MichSt, Purdue, Rutgers);
            dict.Create(Purdue, MichSt, OhioSt, Illinois, Rutgers);
            dict.Create(PennState, Iowa, OhioSt, Indiana, Purdue);
            dict.Create(Rutgers, Minnesota, Wisconsin, Illinois, PennState);
            return dict;
        }

        public static Dictionary<int, int[]> CreateF()
        {
            var dict = new Dictionary<int, int[]>();
            dict.Create(Michigan, MichSt, Wisconsin, Indiana, Rutgers);
            dict.Create(Northwestern, Michigan, MichSt, Wisconsin, OhioSt);
            dict.Create(MichSt, Iowa, Minnesota, Illinois, PennState);
            dict.Create(Iowa, Michigan, Northwestern, Minnesota, Purdue);
            dict.Create(Minnesota, Northwestern, Michigan, Purdue, PennState);
            dict.Create(Wisconsin, MichSt, Iowa, Minnesota, Indiana);

            dict.Create(OhioSt, Michigan, MichSt, Illinois, Rutgers);
            dict.Create(Illinois, Northwestern, Iowa, Indiana, Rutgers);
            dict.Create(Indiana, Northwestern, OhioSt, Purdue, Rutgers);
            dict.Create(Purdue, Wisconsin, OhioSt, Illinois, PennState);
            dict.Create(PennState, Iowa, OhioSt, Illinois, Indiana);
            dict.Create(Rutgers, Minnesota, Wisconsin, Purdue, PennState);
            return dict;
        }

        public static Dictionary<int, int[]> CreateG()
        {
            var dict = new Dictionary<int, int[]>();
            dict.Create(Michigan, Northwestern, MichSt, Purdue, PennState);
            dict.Create(Northwestern, MichSt, Iowa, Wisconsin, Rutgers);
            dict.Create(MichSt, Iowa, Minnesota, Wisconsin, PennState);
            dict.Create(Iowa, Michigan, Minnesota, OhioSt, Purdue);
            dict.Create(Minnesota, Michigan, Northwestern, OhioSt, Illinois);
            dict.Create(Wisconsin, Michigan, Iowa, Minnesota, Indiana);

            dict.Create(OhioSt, Michigan, Illinois, Indiana, Rutgers);
            dict.Create(Illinois, Northwestern, Wisconsin, Indiana, PennState);
            dict.Create(Indiana, MichSt, Iowa, Purdue, Rutgers);
            dict.Create(Purdue, Northwestern, OhioSt, Illinois, Rutgers);
            dict.Create(PennState, Wisconsin, OhioSt, Indiana, Purdue);
            dict.Create(Rutgers, MichSt, Minnesota, Illinois, PennState);
            return dict;
        }


        public static Dictionary<int, int[]> CreateH()
        {
            var dict = new Dictionary<int, int[]>();
            dict.Create(Michigan, Northwestern, MichSt, Illinois, Indiana);
            dict.Create(Northwestern, MichSt, Iowa, Wisconsin, OhioSt);
            dict.Create(MichSt, Iowa, Minnesota, Wisconsin, PennState);
            dict.Create(Iowa, Michigan, Minnesota, PennState, Purdue);
            dict.Create(Minnesota, Michigan, Northwestern, Purdue, Rutgers);
            dict.Create(Wisconsin, Michigan, Iowa, Minnesota, OhioSt);

            dict.Create(OhioSt, Michigan, Illinois, Indiana, Rutgers);
            dict.Create(Illinois, Northwestern, MichSt, Indiana, PennState);
            dict.Create(Indiana, Minnesota, Wisconsin, Purdue, Rutgers);
            dict.Create(Purdue, MichSt, OhioSt, Illinois, Rutgers);
            dict.Create(PennState, Northwestern, OhioSt, Indiana, Purdue);
            dict.Create(Rutgers, Iowa, Wisconsin, Illinois, PennState);
            return dict;
        }

        public static Dictionary<int, int[]> CreateI()
        {
            var dict = new Dictionary<int, int[]>();
            dict.Create(Michigan, MichSt, Wisconsin, Purdue, Rutgers);
            dict.Create(Northwestern, Michigan, Iowa, Indiana, Rutgers);
            dict.Create(MichSt, Northwestern, Wisconsin, OhioSt, PennState);
            dict.Create(Iowa, Michigan, MichSt, Minnesota, Purdue);
            dict.Create(Minnesota, Michigan, Northwestern, MichSt, PennState);
            dict.Create(Wisconsin, Northwestern, Iowa, Minnesota, Indiana);

            dict.Create(OhioSt, Michigan, Iowa, Illinois, Rutgers);
            dict.Create(Illinois, Northwestern, Iowa, Minnesota, Indiana);
            dict.Create(Indiana, MichSt, OhioSt, Purdue, Rutgers);
            dict.Create(Purdue, Wisconsin, OhioSt, Illinois, PennState);
            dict.Create(PennState, Wisconsin, OhioSt, Illinois, Indiana);
            dict.Create(Rutgers, Minnesota, Illinois, Purdue, PennState);
            return dict;
        }

        public static Dictionary<int, int[]> CreateJ()
        {
            var dict = new Dictionary<int, int[]>();
            dict.Create(Michigan, Northwestern, MichSt, Illinois, PennState);
            dict.Create(Northwestern, MichSt, Iowa, Wisconsin, Purdue);
            dict.Create(MichSt, Iowa, Minnesota, Wisconsin, PennState);
            dict.Create(Iowa, Michigan, Minnesota, Rutgers, Purdue);
            dict.Create(Minnesota, Michigan, Northwestern, OhioSt, Indiana);
            dict.Create(Wisconsin, Michigan, Iowa, Minnesota, OhioSt);

            dict.Create(OhioSt, Michigan, Illinois, Indiana, Rutgers);
            dict.Create(Illinois, Northwestern, Wisconsin, Indiana, PennState);
            dict.Create(Indiana, Iowa, Wisconsin, Purdue, Rutgers);
            dict.Create(Purdue, MichSt, OhioSt, Illinois, Rutgers);
            dict.Create(PennState, Northwestern, OhioSt, Indiana, Purdue);
            dict.Create(Rutgers, MichSt, Minnesota, Illinois, PennState);
            return dict;
        }
#else
        public static Dictionary<int, int[]> CreateA()
        {
            var dict = new Dictionary<int, int[]>();
            dict.Create(Indiana, MichSt, OhioSt, Purdue, Wisconsin);
            dict.Create(MichSt, OhioSt, PennState, Wisconsin, Illinois);
            dict.Create(Michigan, Indiana, MichSt, PennState, Iowa);
            dict.Create(OhioSt, Michigan, Rutgers, Purdue, Minnesota);
            dict.Create(PennState, Indiana, OhioSt, Minnesota, Illinois);
            dict.Create(Rutgers, Indiana, MichSt, Michigan, PennState);


            dict.Create(Purdue, Michigan, Minnesota, Illinois, Northwestern);
            dict.Create(Iowa, PennState, Rutgers, Purdue, Wisconsin);
            dict.Create(Minnesota, Michigan, Iowa, Illinois, Northwestern);
            dict.Create(Wisconsin, OhioSt, Purdue, Minnesota, Northwestern);
            dict.Create(Illinois, Rutgers, Iowa, Wisconsin, Northwestern);
            dict.Create(Northwestern, Indiana, MichSt, Rutgers, Iowa);
            return dict;
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return null;
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return null;
        }
        public static Dictionary<int, int[]> CreateD()
        {
            return null;
        }
        public static Dictionary<int, int[]> CreateE()
        {
            return null;
        }
#endif
    }
}
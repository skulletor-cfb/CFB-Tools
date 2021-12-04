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
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateE, CreateE, CreateF, CreateF, CreateC, CreateC, CreateG, CreateG, CreateA, CreateA, CreateH, CreateH, CreateD, CreateD, CreateJ, CreateJ, CreateB, CreateB, CreateI, CreateI };
        //public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateA, CreateA, CreateB, CreateB, CreateC, CreateC, CreateD, CreateD, CreateE, CreateE };
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
            var idx = (Form1.DynastyYear) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(12, RecruitingFixup.Big10Id, "Big10");
            Big10ConferenceSchedule = result.BuildHashSet();
            return result;
        }

#if true

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

        private static void Create(this Dictionary<int, int[]> dict, params int[] values)
        {
            dict[values[0]] = values.Skip(1).ToArray();
        }
    }
}
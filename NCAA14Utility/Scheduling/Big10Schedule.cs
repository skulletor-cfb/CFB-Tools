using System;
using System.Collections.Generic;
using System.Linq;

namespace NCAA14Utility.Scheduling
{
    public class Big10Schedule : ScheduleBase
    {
        public Big10Schedule(int year) : base(year, 12, ScheduleHelpers.Big10, nameof(ScheduleHelpers.Big10))
        {
        }

        protected override Func<Dictionary<int, int[]>>[] Creators => new Func<Dictionary<int, int[]>>[] {
            CreateE, CreateE,
            CreateA, CreateA,
            CreateD, CreateD,
            CreateC, CreateC,
            CreateB, CreateB,
        };

        protected override int InitialYear => 2589;

        // EADBEditor's Big10Locks
        protected override Func<int, int, int?>[] LockChecks => new Func<int, int, int?>[]
        {
            (h, a) => MatchTeams(8, h, a, Illinois, OhioSt),
            (h, a) => MatchTeams(13, h, a, Illinois, Northwestern),
            (h, a) => MatchTeams(13, h, a, Indiana, Purdue),
            (h, a) => MatchTeams(13, h, a, Michigan, OhioSt),
            (h, a) => MatchTeams(11, h, a, Michigan, PennSt),
            (h, a) => MatchTeams(13, h, a, PennSt, MichiganSt),
            (h, a) => MatchTeams(9, h, a, Michigan, MichiganSt),
            (h, a) => MatchTeams(12, h, a, OhioSt, MichiganSt),
            (h, a) => MatchTeams(5, h, a, Iowa, Minnesota),
            (h, a) => MatchTeams(6, h, a, PennSt, OhioSt),
            (h, a) => MatchTeams(12, h, a, PennSt, Rutgers),
            (h, a) => MatchTeams(13, h, a, Wisconsin, Minnesota),
            (h, a) => MatchTeams(10, h, a, Michigan, Minnesota),
            // non-conference: the week after Labor Day weekend
            (h, a) => MatchTeams(this.LaborDayWeek + 1, h, a, Iowa, ISU),
        };

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(Michigan, Minnesota, Iowa,Northwestern),
                PennSt.Create(Rutgers, OhioSt, Michigan, Wisconsin, Illinois),
                OhioSt.Create(Rutgers, Michigan, Purdue, Illinois),
                Michigan.Create(MichiganSt, Wisconsin, Indiana, Northwestern),
                MichiganSt.Create(Rutgers, PennSt, OhioSt, Wisconsin, Purdue),
                Wisconsin.Create(Rutgers, OhioSt, Indiana, Iowa),
                Indiana.Create(OhioSt, MichiganSt, Iowa, Northwestern),
                Purdue.Create(Michigan, Wisconsin, Indiana, Minnesota, Illinois),
                Minnesota.Create(PennSt, MichiganSt, Wisconsin, Indiana, Northwestern),
                Iowa.Create(PennSt, MichiganSt, Purdue, Minnesota, Illinois),
                Northwestern.Create(PennSt, OhioSt, Purdue, Iowa),
                Illinois.Create(Rutgers, Michigan, Indiana, Minnesota, Northwestern),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(OhioSt, MichiganSt, Purdue, Northwestern),
                PennSt.Create(Rutgers, OhioSt, Wisconsin, Indiana, Illinois),
                OhioSt.Create(Michigan, Wisconsin, Purdue, Iowa),
                Michigan.Create(Rutgers, PennSt, MichiganSt, Indiana, Minnesota),
                MichiganSt.Create(PennSt, OhioSt, Wisconsin, Iowa, Northwestern),
                Wisconsin.Create(Rutgers, Michigan, Iowa, Illinois),
                Indiana.Create(Rutgers, OhioSt, Minnesota, Iowa, Northwestern),
                Purdue.Create(PennSt, Michigan, Indiana, Illinois),
                Minnesota.Create(OhioSt, MichiganSt, Wisconsin, Purdue, Illinois),
                Iowa.Create(Michigan, Purdue, Minnesota, Northwestern, Illinois),
                Northwestern.Create(PennSt, Wisconsin, Purdue, Minnesota),
                Illinois.Create(Rutgers, MichiganSt, Indiana, Northwestern),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(Michigan, Purdue, Minnesota, Illinois),
                PennSt.Create(Rutgers, OhioSt, Purdue, Iowa, Northwestern),
                OhioSt.Create(Rutgers, Michigan, Wisconsin, Illinois),
                Michigan.Create(PennSt, MichiganSt, Minnesota, Iowa, Northwestern),
                MichiganSt.Create(OhioSt, Wisconsin, Purdue, Illinois),
                Wisconsin.Create(Michigan, Indiana, Iowa, Northwestern),
                Indiana.Create(Rutgers, PennSt, MichiganSt, Iowa, Northwestern),
                Purdue.Create(Wisconsin, Indiana, Minnesota, Illinois),
                Minnesota.Create(PennSt, OhioSt, MichiganSt, Wisconsin, Indiana),
                Iowa.Create(Rutgers, OhioSt, MichiganSt, Purdue, Minnesota),
                Northwestern.Create(Rutgers, OhioSt, MichiganSt, Purdue),
                Illinois.Create(PennSt, Michigan, Wisconsin, Indiana, Northwestern),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(OhioSt, MichiganSt, Purdue, Minnesota),
                PennSt.Create(Rutgers, OhioSt, Michigan, Purdue, Iowa),
                OhioSt.Create(Michigan, Indiana, Minnesota, Illinois),
                Michigan.Create(Rutgers, Indiana, Minnesota, Iowa, Illinois),
                MichiganSt.Create(PennSt, Wisconsin, Purdue, Northwestern),
                Wisconsin.Create(Rutgers, PennSt, Indiana, Iowa, Illinois),
                Indiana.Create(Rutgers, PennSt, MichiganSt, Northwestern),
                Purdue.Create(OhioSt, Michigan, Wisconsin, Indiana, Illinois),
                Minnesota.Create(PennSt, MichiganSt, Wisconsin, Northwestern),
                Iowa.Create(Rutgers, OhioSt, MichiganSt, Minnesota, Northwestern),
                Northwestern.Create(OhioSt, Michigan, Wisconsin, Purdue),
                Illinois.Create(MichiganSt, Indiana, Minnesota, Iowa, Northwestern),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(MichiganSt, Purdue, Minnesota, Northwestern),
                PennSt.Create(Rutgers, Wisconsin, Indiana, Iowa, Illinois),
                OhioSt.Create(Michigan, MichiganSt, Indiana, Illinois),
                Michigan.Create(MichiganSt, Indiana, Minnesota, Iowa, Illinois),
                MichiganSt.Create(PennSt, Wisconsin, Purdue, Northwestern),
                Wisconsin.Create(Rutgers, OhioSt, Michigan, Indiana, Northwestern),
                Indiana.Create(Rutgers, MichiganSt, Minnesota, Iowa),
                Purdue.Create(PennSt, OhioSt, Michigan, Wisconsin, Indiana),
                Minnesota.Create(PennSt, OhioSt, Purdue, Northwestern),
                Iowa.Create(Rutgers, OhioSt, Purdue, Minnesota, Illinois),
                Northwestern.Create(PennSt, OhioSt, Michigan, Iowa),
                Illinois.Create(Rutgers, MichiganSt, Wisconsin, Minnesota, Northwestern),
            }.Create();
        }
    }
}

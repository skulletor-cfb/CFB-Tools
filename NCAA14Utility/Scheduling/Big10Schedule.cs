using System;
using System.Collections.Generic;

namespace NCAA14Utility.Scheduling
{
    public class Big10Schedule : ScheduleBase
    {
        public Big10Schedule(int year) : base(year, 12, ScheduleHelpers.Big10, nameof(ScheduleHelpers.Big10))
        {
        }

        protected override Func<Dictionary<int, int[]>>[] Creators => new Func<Dictionary<int, int[]>>[] {
            CreateA, CreateB,
            CreateC, CreateA,
            CreateB, CreateC,
            CreateX, CreateY,
            CreateZ, CreateX,
            CreateY, CreateZ,
        };

        protected override int InitialYear => 2574;

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(Michigan, Indiana, Wisconsin, Iowa),
                PennSt.Create(Rutgers, OhioSt, Purdue, Wisconsin),
                MichiganSt.Create(Rutgers, PennSt, Illinois, Minnesota),
                Michigan.Create(PennSt, MichiganSt, Indiana, Iowa),
                OhioSt.Create(Rutgers, Michigan, Illinois, Minnesota),
                Illinois.Create(PennSt, Northwestern, Indiana, Iowa),
                Northwestern.Create(Rutgers, Michigan, OhioSt, Purdue),
                Purdue.Create(MichiganSt, OhioSt, Illinois, Wisconsin),
                Indiana.Create(MichiganSt, Northwestern, Purdue, Minnesota),
                Wisconsin.Create(Michigan, OhioSt, Northwestern, Iowa),
                Minnesota.Create(PennSt, Illinois, Purdue, Wisconsin),
                Iowa.Create(MichiganSt, Northwestern, Indiana, Minnesota),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(Michigan, Illinois, Indiana , Iowa),
                PennSt.Create(Rutgers, OhioSt, Indiana, Wisconsin),
                MichiganSt.Create(PennSt, OhioSt, Illinois, Minnesota),
                Michigan.Create(MichiganSt, Northwestern, Minnesota, Iowa),
                OhioSt.Create(Rutgers, Michigan, Illinois, Indiana),
                Illinois.Create(Michigan, Northwestern, Wisconsin, Minnesota),
                Northwestern.Create(PennSt, MichiganSt, Purdue, Iowa),
                Purdue.Create(Rutgers, PennSt, Michigan, Illinois),
                Indiana.Create(MichiganSt, Northwestern, Purdue, Wisconsin),
                Wisconsin.Create(MichiganSt, OhioSt, Purdue, Iowa),
                Minnesota.Create(Rutgers, Northwestern, Indiana, Wisconsin),
                Iowa.Create(PennSt, OhioSt, Purdue, Minnesota),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(MichiganSt, Illinois, Indiana, Wisconsin),
                PennSt.Create(Rutgers, Michigan, Northwestern, Iowa),
                MichiganSt.Create(PennSt, OhioSt, Wisconsin, Iowa),
                Michigan.Create(MichiganSt, Illinois, Purdue, Minnesota),
                OhioSt.Create(Michigan, Illinois, Indiana, Minnesota),
                Illinois.Create(PennSt, Northwestern, Indiana, Wisconsin),
                Northwestern.Create(Rutgers, MichiganSt, OhioSt, Purdue),
                Purdue.Create(Rutgers, MichiganSt, OhioSt, Minnesota),
                Indiana.Create(PennSt, Michigan, Purdue, Iowa),
                Wisconsin.Create(Michigan, Northwestern, Indiana, Iowa),
                Minnesota.Create(Rutgers, PennSt, Northwestern, Wisconsin),
                Iowa.Create(OhioSt, Illinois, Purdue, Minnesota),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateX()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(Michigan, Northwestern, Minnesota, Iowa),
                PennSt.Create(Rutgers, OhioSt, Illinois, Iowa),
                OhioSt.Create(Rutgers, Michigan, Indiana, Wisconsin),
                Michigan.Create(MichiganSt, Indiana, Illinois, Minnesota),
                MichiganSt.Create(Rutgers, PennSt, Purdue, Iowa),
                Indiana.Create(Rutgers, MichiganSt, Purdue, Wisconsin),
                Purdue.Create(PennSt, OhioSt, Illinois, Minnesota),
                Illinois.Create(MichiganSt, Indiana, Northwestern, Wisconsin),
                Northwestern.Create(PennSt, OhioSt, Michigan, Purdue),
                Wisconsin.Create(PennSt, MichiganSt, Northwestern, Iowa),
                Minnesota.Create(OhioSt, Indiana, Northwestern, Wisconsin),
                Iowa.Create(Michigan, Purdue, Illinois, Minnesota),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateY()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(OhioSt, Indiana, Northwestern, Iowa),
                PennSt.Create(Rutgers, OhioSt, Indiana, Wisconsin),
                OhioSt.Create(Michigan, Purdue, Illinois, Minnesota),
                Michigan.Create(PennSt, MichiganSt, Indiana, Illinois),
                MichiganSt.Create(PennSt, OhioSt, Northwestern, Wisconsin),
                Indiana.Create(MichiganSt, Purdue, Northwestern, Iowa),
                Purdue.Create(Rutgers, Michigan, Illinois, Wisconsin),
                Illinois.Create(Rutgers, MichiganSt, Northwestern, Minnesota),
                Northwestern.Create(PennSt, Purdue, Minnesota, Iowa),
                Wisconsin.Create(Rutgers, Michigan, Illinois, Iowa),
                Minnesota.Create(PennSt, MichiganSt, Indiana, Wisconsin),
                Iowa.Create(OhioSt, Michigan, Purdue, Minnesota),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateZ()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Rutgers.Create(Michigan, Purdue, Northwestern, Wisconsin),
                PennSt.Create(Rutgers, OhioSt, Indiana, Illinois),
                OhioSt.Create(Michigan, Indiana, Illinois, Iowa),
                Michigan.Create(PennSt, MichiganSt, Northwestern, Minnesota),
                MichiganSt.Create(Rutgers, OhioSt, Purdue, Minnesota),
                Indiana.Create(MichiganSt, Purdue, Northwestern, Iowa),
                Purdue.Create(PennSt, Michigan, Illinois, Wisconsin),
                Illinois.Create(Rutgers, Indiana, Northwestern, Minnesota),
                Northwestern.Create(OhioSt, MichiganSt, Wisconsin, Iowa),
                Wisconsin.Create(OhioSt, Michigan, Indiana, Iowa),
                Minnesota.Create(Rutgers, PennSt, Purdue, Wisconsin),
                Iowa.Create(PennSt, MichiganSt, Illinois, Minnesota),
            }.Create();
        }
    }
}

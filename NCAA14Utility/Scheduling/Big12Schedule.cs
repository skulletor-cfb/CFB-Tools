using System;
using System.Collections.Generic;
using System.Linq;

namespace NCAA14Utility.Scheduling
{
    public class Big12Schedule : ScheduleBase
    {
        public Big12Schedule(int year) : base(year, 12, ScheduleHelpers.Big12, nameof(ScheduleHelpers.Big12))
        {
        }

        protected override Func<Dictionary<int, int[]>>[] Creators => new Func<Dictionary<int, int[]>>[] {
            CreateNDY, CreateNDA,
            CreateNDZ, CreateNDY,
            CreateNDAPrime, CreateNDZ,
            CreateNDY, CreateNDAPrime,
            CreateNDZ, CreateNDY,
            CreateNDA, CreateNDZ,
        };

        protected override int InitialYear => 2587;        

   
        protected override Func<int, int, int?>[] LockChecks => new Func<int, int, int?>[]
        {
            (h, a) => MatchTeams(13, h, a, TCU, Baylor),
            (h, a) => MatchTeams(13, h, a, TT, Texas),
            (h, a) => MatchTeams(13, h, a, KU, KSU),
            (h, a) => MatchTeams(13, h, a, Oklahoma, OkSt),
            (h, a) => MatchTeams(13, h, a, Colorado, Nebraska),
            (h, a) => MatchTeams(12, h, a, Nebraska, Oklahoma),
            (h, a) => MatchTeams(12, h, a, ISU, KSU),
            (h, a) => MatchTeams(this.WeekOf(this.Calendar.RedRiverShowdown), h, a, Texas, Oklahoma),
            // Iowa State and Cincinnati end the season when they play
            (h, a) => MatchTeams(13, h, a, Cincy, ISU),
            (h, a) => MatchTeams(7, h, a, Baylor, TT),
            (h, a) => MatchTeams(12, h, a, Colorado, OkSt),
            // non-conference
            (h, a) => MatchTeams(0, h, a, Colorado, CSU),
        };

        public static Dictionary<int, int[]> CreateNDY()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Texas.Create(Nebraska, ISU, TCU, TT),
                Oklahoma.Create(Texas, OkSt, KU, Baylor),
                OkSt.Create(Texas, Colorado, KSU, TT),
                Colorado.Create(Oklahoma, Nebraska, ISU, Baylor),
                Nebraska.Create(Oklahoma, OkSt, KSU, Cincy),
                KU.Create(OkSt, Colorado, Nebraska, TCU),
                KSU.Create(Texas, KU, Cincy, Baylor),
                ISU.Create(Oklahoma, KSU, Cincy, TCU),
                Cincy.Create(Oklahoma, Colorado, KU, TT),
                TCU.Create(OkSt, Nebraska, Cincy, TT),
                Baylor.Create(Texas, KU, ISU, TCU),
                TT.Create(Colorado, KSU, ISU, Baylor),

            }.Create();
        }

        public static Dictionary<int, int[]> CreateNDA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Texas.Create(Nebraska, ISU, Cincy, TT),
                Oklahoma.Create(Texas, OkSt, KU, Baylor),
                OkSt.Create(Colorado, KSU, TCU, TT),
                Colorado.Create(Texas, Nebraska, KSU, Cincy),
                Nebraska.Create(Oklahoma, KSU, ISU, TCU),
                KU.Create(Texas, Nebraska, ISU, Baylor),
                KSU.Create(Oklahoma, KU, TCU, TT),
                ISU.Create(OkSt, Colorado, KSU, Cincy),
                Cincy.Create(Oklahoma, OkSt, KU, Baylor),
                TCU.Create(Oklahoma, Colorado, Cincy, TT),
                Baylor.Create(Texas, OkSt, ISU, TCU),
                TT.Create(Colorado, Nebraska, KU, Baylor),

            }.Create();
        }

        public static Dictionary<int, int[]> CreateNDZ()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Texas.Create(Colorado, KU, TCU, TT),
                Oklahoma.Create(Texas, OkSt, KSU, TCU),
                OkSt.Create(Colorado, Nebraska, ISU, TT),
                Colorado.Create(Oklahoma, Nebraska, KSU, Baylor),
                Nebraska.Create(Oklahoma, ISU, Cincy, Baylor),
                KU.Create(OkSt, Colorado, Nebraska, ISU),
                KSU.Create(Texas, KU, Cincy, TCU),
                ISU.Create(Oklahoma, KSU, Cincy, TT),
                Cincy.Create(Texas, OkSt, Baylor, TT),
                TCU.Create(Colorado, KU, ISU, Cincy),
                Baylor.Create(Texas, OkSt, KSU, TCU),
                TT.Create(Oklahoma, Nebraska, KU, Baylor),

            }.Create();
        }

        public static Dictionary<int, int[]> CreateNDAPrime()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Texas.Create(OkSt, Colorado, KU, TT),
                Oklahoma.Create(Texas, OkSt, KU, TCU),
                OkSt.Create(Colorado, KSU, Cincy, Baylor),
                Colorado.Create(Nebraska, KSU, ISU, TT),
                Nebraska.Create(Oklahoma, ISU, TCU, Baylor),
                KU.Create(Nebraska, ISU, Cincy, TT),
                KSU.Create(Oklahoma, Nebraska, KU, TCU),
                ISU.Create(Texas, OkSt, KSU, Cincy),
                Cincy.Create(Texas, Oklahoma, Colorado, Baylor),
                TCU.Create(OkSt, Colorado, Cincy, TT),
                Baylor.Create(Texas, KU, ISU, TCU),
                TT.Create(Oklahoma, Nebraska, KSU, Baylor),

            }.Create();
        }
    }
}

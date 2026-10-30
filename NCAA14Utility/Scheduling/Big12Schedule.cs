using System;
using System.Collections.Generic;
using System.Linq;

namespace NCAA14Utility.Scheduling
{
    public class Big12Schedule : ScheduleBase
    {
        public Big12Schedule(int year) : base(year, 15, ScheduleHelpers.Big12, nameof(ScheduleHelpers.Big12))
        {
        }

        protected override Func<Dictionary<int, int[]>>[] Creators => new Func<Dictionary<int, int[]>>[] {
            CreateA, CreateA,
            CreateB, CreateB,
        };

        protected override int InitialYear => 2578;


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

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Oklahoma.Create(Texas, Baylor, UCF, OkSt),
                Texas.Create(TT, Houston, Cincy, KSU),
                TT.Create(Baylor, SMU, UCF, ISU),
                Baylor.Create(TCU, UCF, ISU, Colorado),
                TCU.Create(Texas, SMU, Cincy, KU),
                SMU.Create(Oklahoma, Baylor, Houston, ISU),
                Houston.Create(TCU, Cincy, Nebraska, OkSt),
                UCF.Create(SMU, Houston, KSU, Colorado),
                Cincy.Create(UCF, KU, Colorado, OkSt),
                ISU.Create(Oklahoma, Cincy, KSU, Nebraska),
                KSU.Create(Baylor, SMU, KU, Nebraska),
                KU.Create(Oklahoma, TT, Houston, Colorado),
                Nebraska.Create(Oklahoma, TT, TCU, KU),
                Colorado.Create(Texas, ISU, Nebraska, OkSt),
                OkSt.Create(Texas, TT, TCU, KSU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Oklahoma.Create(Texas, TT, TCU, OkSt),
                Texas.Create(TT, SMU, ISU, KU),
                TT.Create(Baylor, Houston, KSU, Colorado),
                Baylor.Create(Texas, TCU, Cincy, Nebraska),
                TCU.Create(TT, SMU, UCF, ISU),
                SMU.Create(Houston, Cincy, Nebraska, Colorado),
                Houston.Create(Oklahoma, Baylor, KSU, Colorado),
                UCF.Create(Texas, Houston, Nebraska, OkSt ),
                Cincy.Create(Oklahoma, TT, UCF, ISU),
                ISU.Create(Houston, UCF, KSU, OkSt),
                KSU.Create(Oklahoma, TCU , Cincy, KU),
                KU.Create(Baylor, SMU, UCF, ISU),
                Nebraska.Create(Texas, Cincy, KU, OkSt),
                Colorado.Create(Oklahoma, TCU ,KSU, Nebraska),
                OkSt.Create(Baylor, SMU, KU, Colorado),
            }.Create();
        }
    }
}
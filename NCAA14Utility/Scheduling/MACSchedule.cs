using System;
using System.Collections.Generic;

namespace NCAA14Utility.Scheduling
{
    public class MACSchedule : ScheduleBase
    {
        public MACSchedule(int year) : base(year, 12, ScheduleHelpers.MAC, nameof(ScheduleHelpers.MAC))
        {
        }

        protected override Func<Dictionary<int, int[]>>[] Creators => new Func<Dictionary<int, int[]>>[] {
            CreateA, CreateB,
            CreateC, CreateA,
            CreateB, CreateC,
        };

        protected override int InitialYear => 2491;

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Akron.Create(MiamiOH, Ohio, Buffalo, CMU),
                KentSt.Create(Akron, NIU, Buffalo, WMU),
                BGSU.Create(KentSt, NIU, MiamiOH, EMU),
                Toledo.Create(Akron, BGSU, BallSt, CMU),
                NIU.Create(Akron, Toledo, Ohio, WMU),
                BallSt.Create(KentSt, BGSU, NIU, EMU),
                MiamiOH.Create(Toledo, BallSt, Buffalo, CMU),
                Ohio.Create(KentSt, Toledo, MiamiOH, WMU),
                Buffalo.Create(BGSU, BallSt, Ohio, EMU),
                EMU.Create(KentSt, NIU, Ohio, CMU),
                CMU.Create(BGSU, BallSt, Buffalo, WMU),
                WMU.Create(Akron, Toledo, MiamiOH, EMU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Akron.Create(NIU, Ohio, Buffalo, EMU),
                KentSt.Create(Akron, BallSt, Buffalo, CMU),
                BGSU.Create(Akron, KentSt, NIU, WMU),
                Toledo.Create(KentSt, BGSU, BallSt, EMU),
                NIU.Create(Toledo, MiamiOH, Buffalo, CMU),
                BallSt.Create(Akron, NIU, Ohio, WMU),
                MiamiOH.Create(KentSt, BGSU, BallSt, EMU),
                Ohio.Create(BGSU, Toledo, MiamiOH, CMU),
                Buffalo.Create(Toledo, MiamiOH, Ohio, WMU),
                EMU.Create(KentSt, NIU, Ohio, CMU),
                CMU.Create(BGSU, BallSt, Buffalo, WMU),
                WMU.Create(Akron, Toledo, MiamiOH, EMU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Akron.Create(BallSt, MiamiOH, Buffalo, EMU),
                KentSt.Create(Akron, NIU, Ohio, CMU),
                BGSU.Create(Akron, KentSt, Buffalo, WMU),
                Toledo.Create(Akron, KentSt, BGSU, EMU),
                NIU.Create(Toledo, MiamiOH, Ohio, CMU),
                BallSt.Create(BGSU, NIU , Buffalo, WMU),
                MiamiOH.Create(KentSt, Toledo, BallSt, EMU),
                Ohio.Create(BGSU, BallSt, MiamiOH, CMU),
                Buffalo.Create(Toledo, NIU, Ohio, WMU),
                EMU.Create(BGSU, BallSt, Buffalo, CMU),
                CMU.Create(Akron, Toledo, MiamiOH, WMU),
                WMU.Create(KentSt, NIU, Ohio, EMU),
            }.Create();
        }
    }
}

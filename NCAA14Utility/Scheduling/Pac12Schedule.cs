using System;
using System.Collections.Generic;
using System.Linq;

namespace NCAA14Utility.Scheduling
{
    public class Pac12Schedule : ScheduleBase
    {
        public Pac12Schedule(int year) : base(year, 12, ScheduleHelpers.Pac12, nameof(ScheduleHelpers.Pac12))
        {
        }

        protected override Func<Dictionary<int, int[]>>[] Creators => new Func<Dictionary<int, int[]>>[] {
            CreateD, CreateD,
            CreateC, CreateC,
            CreateB, CreateB,
            CreateA, CreateA,
        };

        protected override int InitialYear => 2589;

        // EADBEditor's Pac12Locks
        protected override Func<int, int, int?>[] LockChecks => new Func<int, int, int?>[]
        {
            (h, a) => MatchTeams(13, h, a, Washington, WSU),
            (h, a) => MatchTeams(13, h, a, Utah, BYU),
            (h, a) => MatchTeams(13, h, a, Oregon, OrSt),
            (h, a) => MatchTeams(13, h, a, Arizona, ASU),
            (h, a) => MatchTeams(this.IsEvenYear ? 13 : 12, h, a, UCLA, USC),
            (h, a) => MatchTeams(this.IsEvenYear ? 12 : 13, h, a, Cal, Stanford),
            // non-conference
            (h, a) => MatchTeams(0, h, a, Utah, UtahSt),
        };

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USC.Create(UCLA, Stanford, Washington, OrSt, BYU),
                UCLA.Create(Cal, WSU, Oregon, Utah),
                Cal.Create(USC, Washington, OrSt, Arizona, BYU),
                Stanford.Create(UCLA, Cal, WSU, ASU),
                WSU.Create(Cal, Oregon, Arizona, Utah),
                Washington.Create(Stanford, WSU, OrSt, ASU, BYU),
                Oregon.Create(USC, Stanford, Washington,ASU),
                OrSt.Create(UCLA, WSU, Oregon, Arizona, Utah),
                ASU.Create(UCLA, Cal, OrSt, Arizona, Utah),
                Arizona.Create(USC, Stanford, Oregon, BYU),
                Utah.Create(USC, Stanford, Washington, Arizona),
                BYU.Create(UCLA, WSU, Oregon, ASU, Utah),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USC.Create(UCLA, Stanford, Oregon, ASU, Utah),
                UCLA.Create(Cal, WSU, OrSt, Arizona, BYU),
                Cal.Create(USC, Washington, Oregon, ASU),
                Stanford.Create(UCLA, Cal, OrSt, Utah),
                WSU.Create(USC, Cal, Oregon, ASU, BYU),
                Washington.Create(UCLA, Stanford, WSU, OrSt, Arizona),
                Oregon.Create(Stanford, Washington, ASU, Utah),
                OrSt.Create(USC, WSU, Oregon, Arizona, BYU),
                ASU.Create(UCLA, Washington, Arizona, Utah),
                Arizona.Create(Cal, Stanford, Oregon, BYU),
                Utah.Create(Cal, WSU, OrSt, Arizona),
                BYU.Create(USC, Stanford, Washington, ASU, Utah),
            }.Create();
        }
        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USC.Create(UCLA, Stanford, Washington, ASU, BYU),
                UCLA.Create(Cal, WSU, Oregon, Arizona),
                Cal.Create(USC, Washington, OrSt, BYU),
                Stanford.Create(UCLA, Cal, OrSt, Utah),
                WSU.Create(USC, Stanford, Oregon, ASU),
                Washington.Create(UCLA, WSU, OrSt, Arizona, Utah),
                Oregon.Create(Cal, Stanford, Washington, Arizona, BYU),
                OrSt.Create(USC, WSU, Oregon, ASU, Utah),
                ASU.Create(UCLA, Stanford, Washington, Arizona, Utah),
                Arizona.Create(USC, Cal, WSU, BYU),
                Utah.Create(UCLA, Cal, Oregon, Arizona),
                BYU.Create(Stanford, WSU, OrSt, ASU, Utah),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USC.Create(UCLA, Stanford, Washington, ASU, Utah),
                UCLA.Create(Cal, Washington, Oregon, Arizona),
                Cal.Create(USC, WSU, OrSt, Utah, BYU),
                Stanford.Create(UCLA, Cal, Washington, ASU),
                WSU.Create(USC, Stanford, Oregon, Arizona),
                Washington.Create(WSU, OrSt, Utah, BYU),
                Oregon.Create(USC, Cal, Washington, BYU),
                OrSt.Create(UCLA, Stanford, WSU, Oregon, ASU),
                ASU.Create(Cal, WSU, Oregon, Arizona, Utah),
                Arizona.Create(USC, Stanford, Washington, OrSt, BYU),
                Utah.Create(UCLA, WSU, Oregon, Arizona),
                BYU.Create(UCLA, Stanford, OrSt, ASU, Utah),
            }.Create();
        }
    }
}

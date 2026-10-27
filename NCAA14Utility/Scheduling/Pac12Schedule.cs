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
            CreateA, CreateB,
            CreateX, CreateY,
            CreateB, CreateA,
            CreateY, CreateX,
        };

        protected override int InitialYear => 2587;

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
                Washington.Create(WSU, BYU, Stanford, Arizona),
                WSU.Create(OrSt, Utah, Cal, ASU),
                OrSt.Create(Washington, Oregon, BYU, USC),
                Oregon.Create(Washington, WSU, Utah, UCLA),
                BYU.Create(WSU, Oregon, Utah, Arizona),
                Utah.Create(Washington, OrSt, UCLA, ASU),

                Stanford.Create(OrSt, BYU, Cal, UCLA),
                Cal.Create(Oregon, Utah, USC, ASU),
                USC.Create(Washington, BYU, Stanford, UCLA),
                UCLA.Create(WSU, Cal, Arizona, ASU),
                Arizona.Create(OrSt, Stanford, Cal, USC),
                ASU.Create(Oregon, Stanford, USC, Arizona),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Washington.Create(WSU, BYU, Cal , ASU),
                WSU.Create(OrSt, Utah, Stanford, USC),
                OrSt.Create(Washington, Oregon, BYU, UCLA),
                Oregon.Create(Washington, WSU, Utah, Arizona),
                BYU.Create(WSU, Oregon, Utah, ASU),
                Utah.Create(Washington, OrSt, USC, Arizona),

                Stanford.Create(Oregon, Utah, Cal, UCLA),
                Cal.Create(OrSt, BYU, USC, ASU),
                USC.Create(Oregon, Stanford, UCLA, ASU),
                UCLA.Create(Washington, BYU, Cal , Arizona),
                Arizona.Create(WSU, Stanford, Cal, USC),
                ASU.Create(OrSt, Stanford, UCLA, Arizona),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateX()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Washington.Create(WSU, ASU, Stanford, Utah),
                WSU.Create(OrSt, Arizona, Cal, BYU),
                OrSt.Create(Washington, Oregon, ASU, USC),
                Oregon.Create(Washington, WSU, Arizona, UCLA),
                ASU.Create(WSU, Oregon, Arizona, Utah),
                Arizona.Create(Washington, OrSt, UCLA, BYU),

                Stanford.Create(OrSt, ASU, Cal, UCLA),
                Cal.Create(Oregon, Arizona, USC, BYU),
                USC.Create(Washington, ASU, Stanford, UCLA),
                UCLA.Create(WSU, Cal, Utah, BYU),
                Utah.Create(OrSt, Stanford, Cal, USC),
                BYU.Create(Oregon, Stanford, USC, Utah),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateY()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Washington.Create(WSU, ASU, Cal , BYU),
                WSU.Create(OrSt, Arizona, Stanford, USC),
                OrSt.Create(Washington, Oregon, ASU, UCLA),
                Oregon.Create(Washington, WSU, Arizona, Utah),
                ASU.Create(WSU, Oregon, Arizona, BYU),
                Arizona.Create(Washington, OrSt, USC, Utah),

                Stanford.Create(Oregon, Arizona, Cal, UCLA),
                Cal.Create(OrSt, ASU, USC, BYU),
                USC.Create(Oregon, Stanford, UCLA, BYU),
                UCLA.Create(Washington, ASU, Cal , Utah),
                Utah.Create(WSU, Stanford, Cal, USC),
                BYU.Create(OrSt, Stanford, UCLA, Utah),
            }.Create();
        }
    }
}

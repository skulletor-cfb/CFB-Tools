using System;
using System.Collections.Generic;
using System.Linq;

namespace NCAA14Utility.Scheduling
{
    public class AmericanSchedule : ScheduleBase
    {
        public AmericanSchedule(int year) : base(year, 14, ScheduleHelpers.American, nameof(ScheduleHelpers.American))
        {
        }

        protected override Func<Dictionary<int, int[]>>[] Creators => new Func<Dictionary<int, int[]>>[] {
            CreateB, CreateB,
            CreateA, CreateA,
        };

        protected override int InitialYear => 2589;

        // EADBEditor's AmericanLocks; the featured rivalries' weeks are shuffled, seeded by the year
        private int[] featuredRivalryWeeks;

        private int[] FeaturedRivalryWeeks => this.featuredRivalryWeeks ??
            (this.featuredRivalryWeeks = new[] { 12, 11, 10, 9, 8, 7 }.OrderBy(_ => new Random(this.Year).Next()).ToArray());

        protected override Func<int, int, int?>[] LockChecks => new Func<int, int, int?>[]
        {
            (h, a) => MatchTeams(this.FeaturedRivalryWeeks[0], h, a, NT, UTSA),
            (h, a) => MatchTeams(this.FeaturedRivalryWeeks[1], h, a, Rice, Tulsa),
            (h, a) => MatchTeams(this.FeaturedRivalryWeeks[2], h, a, ECU, Charlotte),
            (h, a) => MatchTeams(this.FeaturedRivalryWeeks[3], h, a, Memphis, UAB),
            (h, a) => MatchTeams(this.FeaturedRivalryWeeks[4], h, a, USF, FAU),
            (h, a) => MatchTeams(13, h, a, Army, Navy),
            (h, a) => MatchTeams(this.FeaturedRivalryWeeks[5], h, a, NT, UTSA),
        };

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                NT.Create(Tulsa, UAB, Army, ECU),
                UTSA.Create(NT,Tulsa, Memphis, Navy),
                Rice.Create(UTSA, Tulane, UAB, Temple),
                Tulsa.Create(Rice, Memphis, Navy, ECU),
                Tulane.Create(NT, UAB, Temple, USF),
                Memphis.Create(NT, Tulane, Army, Charlotte),
                UAB.Create(Memphis, Navy, FAU, ECU),
                Navy.Create(Tulane,Army,USF,ECU),
                Army.Create(Rice, Temple, FAU, Charlotte),
                Temple.Create(UTSA, Navy, USF, Charlotte),
                FAU.Create(NT, Rice, Memphis, Temple),
                USF.Create(UTSA, Tulsa, UAB, FAU),
                ECU.Create(Rice, Army, FAU, Charlotte),
                Charlotte.Create(UTSA, Tulsa, Tulane, USF),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                NT.Create(Rice, Memphis, Navy, USF),
                UTSA.Create(NT, Tulsa, Tulane, Army),
                Rice.Create(UTSA, Tulane, Navy, Charlotte),
                Tulsa.Create(NT, Rice, Tulane, Army),
                Tulane.Create(UAB, Army, FAU, ECU),
                Memphis.Create(Rice, Tulane, Temple, ECU),
                UAB.Create(UTSA, Tulsa, Memphis, Temple),
                Navy.Create(Memphis, Army, FAU, ECU),
                Army.Create(UAB, Temple, USF, Charlotte),
                Temple.Create(NT, Tulsa, Navy , USF),
                FAU.Create(UTSA, Tulsa, UAB, Charlotte),
                USF.Create(Rice, Memphis, FAU, ECU),
                ECU.Create(UTSA, Temple, FAU, Charlotte),
                Charlotte.Create(NT, UAB, Navy, USF),
            }.Create();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace NCAA14Utility
{
    public class SeasonCalendar
    {
        public bool IsLaborDayWeekendFirstWeek { get; }
        public DateTime[] Weeks { get; }
        public DateTime FirstDayOfSeason { get; }
        public DateTime ThirdSaturdayInOctober { get; }
        public DateTime LaborDay { get; }
        public DateTime Thanksgiving { get; }
        public DateTime FinalDayOfSeason { get; }
        public DateTime TexasStateFairStartDate { get; }
        public DateTime RedRiverShowdown { get; }
        public DateTime ThirdSaturdayInSeptember { get; }
        public DateTime LastSaturdayInSeptember { get; }
        public int Year { get; }
        public SeasonCalendar(int year)
        {
            Year = year;
            Weeks = new DateTime[14];
            ThirdSaturdayInOctober = year.GetThirdSaturdayInOctober();
            LaborDay = year.GetLaborDay();
            Thanksgiving = year.GetThanksgivingDate();
            FinalDayOfSeason = Thanksgiving.AddDays(2);
            TexasStateFairStartDate = year.GetTexasStateFairStartDate();
            ThirdSaturdayInSeptember = year.GetThirdSaturdayInSeptember();
            LastSaturdayInSeptember = year.GetLastSaturdayInSeptember();
            var rrs = TexasStateFairStartDate.AddDays(15);
            if (rrs.Day >= 15)
            {
                rrs = rrs.AddDays(-7);
            }
            RedRiverShowdown = rrs;
            FirstDayOfSeason = FinalDayOfSeason.AddDays(-7 * 13);

            var curr = FirstDayOfSeason;
            for (int i = 0; i < Weeks.Length; i++)
            {
                Weeks[i] = curr;
                curr = curr.AddDays(7);
            }

            IsLaborDayWeekendFirstWeek = LaborDay < Weeks[1];
        }

        public bool IsOctober(int week)
        {
            var date = Weeks[week];
            return date.Month == 10;
        }

        public bool IsAugustSeptember(int week)
        {
            var date = Weeks[week];
            return date.Month == 8 || date.Month == 9;
        }
        public bool IsNovember(int week)
        {
            var date = Weeks[week];
            return date.Month >= 11;
        }
        public DateTime GetDate(int week, int day)
        {
            var saturday = Weeks[week];
            return saturday.AddDays(day - 5);
        }
    }
}

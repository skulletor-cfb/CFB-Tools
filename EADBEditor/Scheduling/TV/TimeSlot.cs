using System;
using System.Collections.Generic;

namespace EA_DB_Editor.Scheduling
{
    public class TimeSlot
    {
        private static readonly Dictionary<int, string> Days = new Dictionary<int, string>
        {
            [0] = "Monday",
            [1] = "Tuesday",
            [2] = "Wednesday",
            [3] = "Thursday",
            [4] = "Friday",
            [5] = "Saturday",
            [6] = "Sunday",
        };

        public int Day { get; }
        public int Hour { get; }
        public int Minute { get; }
        public bool AM { get; }
        public int Week { get;  }
        public string DayOfWeek => Days[Day];
        public string Month =>TelevisionScheduler.CurrentSeason.Weeks[Week].AddDays(Day - 5).ToString("MMMM");
        public int DayOfMonth => TelevisionScheduler.CurrentSeason.Weeks[Week].AddDays(Day - 5).Day;
        public TimeSlot(int hour, int minute, int week , bool am = false, int day = 5)
        {
            Hour = hour;
            Minute = minute;
            AM = am;
            Day = day;
            Week = week;
        }

        public TimeSlot(GameTimeOfDay gtod, int week, int day)
        {
            Hour = gtod.Hour;
            Minute = gtod.Minute;
            AM = gtod.AM;
            Day = day;
            Week = week;
        }

        public string ToTimeString()
        {
            var am = AM ? "AM" : "PM";
            var min = Minute < 10 ? "0" + Minute : Minute.ToString();
            return $"{this.Hour}:{min}{am}";
        }

        public override string ToString()
        {
            return $"Week {this.Week}-{Days[this.Day].Substring(0, 3)}-{this.ToTimeString()}";
        }

        public override bool Equals(object obj)
        {
            return obj is TimeSlot other &&
                this.Hour == other.Hour &&
                this.Minute == other.Minute &&
                this.AM == other.AM &&
                this.Day == other.Day &&
                this.Week == other.Week;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var arr = new int[] { this.Hour, this.Minute, this.Day, this.AM ? 101 : 103, this.Week };
                var code = 23;

                foreach (var item in arr)
                {
                    code = code * 17 + item;
                }

                return code;
            }
        }

        public int GTOD
        {
            get
            {
                if (AM || Hour == 12)
                {
                    return (60 * Hour) + Minute;
                }

                return (60 * (12 + Hour)) + Minute;
            }
        }

        public string ToGTOD() => GTOD.ToString();
    }
}
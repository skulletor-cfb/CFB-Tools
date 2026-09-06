using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor.Scheduling
{
    public class TimeSlot
    {
        private static readonly Dictionary<int, string> Days = new Dictionary<int, string>
        {
            [0] = "Mon",
            [1] = "Tue",
            [2] = "Wed",
            [3] = "Thur",
            [4] = "Fri",
            [5] = "Sat",
            [6] = "Sun",
        };

        public static readonly TimeSlot ShamrockSeries = new TimeSlot(8, 7);// 807pm
        public static readonly TimeSlot MayhemAtMBS = new TimeSlot(7, 33); // 733pm
        public static readonly TimeSlot OysterBowl = new TimeSlot(7, 17); //717 pm
        public static readonly TimeSlot JohnnyMajorsClassic = new TimeSlot(7, 37); //737pm

        public int Day { get; }
        public int Hour { get; }
        public int Minute { get; }
        public bool AM { get; }
        public int? Week { get; }
        public TimeSlot(int hour, int minute, int? week = null, bool am = false, int day = 5)
        {
            Hour = hour;
            Minute = minute;
            AM = am;
            Day = day;
            Week = week;
        }

        public override string ToString()
        {
            var am = AM ? "AM" : "PM";
            var min = Minute < 10 ? "0" + Minute : Minute.ToString();
            return $"Week {this.Week}-{Days[this.Day]}-{this.Hour}:{min}{am}";
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
                var arr = new int[] { this.Hour, this.Minute, this.Day, this.AM ? 101 : 103, this.Week ?? 113 };
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

        public string ToGTOD()
        {
            var hourMod = AM ? Hour : (12 + Hour);
            var result = hourMod * 60 + Minute;
            return result.ToString();
        }
    }
}

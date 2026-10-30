#nullable disable
namespace ScheduleMaker.TV
{
    /// <summary>
    /// A kickoff: week, day (NCAA 14's numbering, 0 Monday ... 5 Saturday ... 6 Sunday) and Eastern time. CFB 27 only has a
    /// fixed set of kickoff times (GameTimeRange: T1100 ... T2300), so <see cref="ToGameTimeRange"/> snaps to the nearest one.
    /// CFB 27's week runs Tuesday to Monday, so a week's Monday is the one after its Saturday (NCAA 14's was the one before).
    /// </summary>
    public class TimeSlot
    {
        public static readonly string[] Days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        public int Day { get; }
        public int Hour { get; }
        public int Minute { get; }
        public bool AM { get; }
        public int Week { get; }
        public string DayOfWeek => Days[Day];
        public string Month => Date.ToString("MMMM");
        public int DayOfMonth => Date.Day;
        public DateTime Date => TelevisionScheduler.CurrentSeason.Weeks[Week].AddDays(DaysFromSaturday(Day));

        /// <summary>Where a day falls in CFB 27's week (Tuesday ... Monday), counted from the week's Saturday: Tuesday -4, Monday +2.</summary>
        public static int DaysFromSaturday(int day) => day == 0 ? 2 : day - 5;

        public TimeSlot(int hour, int minute, int week, bool am = false, int day = 5)
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

        public override int GetHashCode() => HashCode.Combine(Hour, Minute, Day, AM, Week);

        /// <summary>Minutes after midnight.</summary>
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

        /// <summary>The CFB 27 kickoff (a GameTimeRange name such as "T1930") closest to this time; ties go to the earlier one.</summary>
        public string ToGameTimeRange() => TelevisionScheduler.NearestGameTime(GTOD).Name;

        /// <summary>The time the game will show in CFB 27 (<see cref="ToGameTimeRange"/>), as "7:30PM".</summary>
        public string ToGameTimeString() =>
            new TimeSlot(new GameTimeOfDay(TelevisionScheduler.NearestGameTime(GTOD).Minutes), Week, Day).ToTimeString();
    }

    public class GameTimeOfDay
    {
        public int Hour { get; }
        public int Minute { get; }
        public bool AM { get; }
        public GameTimeOfDay(int gtod)
        {
            Hour = gtod / 60;
            Minute = gtod % 60;
            AM = Hour < 12;

            if (Hour > 12)
            {
                Hour -= 12;
            }
        }

        public GameTimeOfDay(DateTime time)
        {
            Hour = time.Hour;
            Minute = time.Minute;
            AM = Hour < 12;

            if (Hour > 12)
            {
                Hour -= 12;
            }
        }

        public int Value => Hour * 60 + Minute;

        public bool Equals(int gtod) => this.Value == gtod;
    }
}

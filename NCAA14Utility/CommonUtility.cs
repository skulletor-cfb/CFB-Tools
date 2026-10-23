using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCAA14Utility
{
    public static class CommonUtility
    {
        /// <summary>
        /// Returns true if a check passes the threshold
        /// </summary>
        /// <param name="threshold">% success, so if we pass in 10, there's a 10% chance this check passes</param>
        /// <returns></returns>
        public static bool Check(int threshold)
        {
            return Rand100() < threshold;
        }

        public static T Check<T>(int[] pct, params T[] values)
        {
            var val = Rand100();

            for (int i = 0; i < pct.Length; i++)
            {
                if (val < pct.Take(1 + i).Sum())
                {
                    return values[i];
                }
            }

            return values.Last();
        }

        /// <summary>
        /// randomly returns a choice from a set
        /// </summary>
        /// <param name="choices"></param>
        /// <returns></returns>
        public static T Choose<T>(this T[] choices)
        {
            var guid = Guid.NewGuid().ToByteArray().Take(4).ToArray();
            int i = BitConverter.ToInt32(guid, 0) & 0x7fffffff;
            return choices[i % choices.Length];
        }

        public static T Choose<T>(this List<T> choices)
        {
            var guid = Guid.NewGuid().ToByteArray().Take(4).ToArray();
            int i = BitConverter.ToInt32(guid, 0) & 0x7fffffff;
            return choices[i % choices.Count];
        }


        public static int Rand100()
        {
            return 100.Next();
        }

        /// <summary>Returns a random integer that is within a specified range.</summary>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/>
        /// but not <paramref name="maxValue"/>. If minValue equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
        public static int Next(this int maxValue)
        {
            var guid = Guid.NewGuid().ToByteArray().Take(4).ToArray();
            int i = BitConverter.ToInt32(guid, 0) & 0x7fffffff;
            return i % maxValue;
        }

        #region calendar utlities
        public static DateTime GetThanksgivingDate(this int year)
        {

            for (int i = 22; i <= 30; i++)
            {
                var testDate = new DateTime(year, 11, i, 0, 0, 0);
                if (testDate.DayOfWeek == DayOfWeek.Thursday)
                {
                    return testDate;
                }
            }

            return default;
        }

        public static DateTime GetLaborDay(this int year)
        {

            for (int i = 1; i <= 7; i++)
            {
                var testDate = new DateTime(year, 9, i, 0, 0, 0);
                if (testDate.DayOfWeek == DayOfWeek.Monday)
                {
                    return testDate;
                }
            }

            return default;
        }

        public static DateTime GetThirdSaturdayInOctober(this int year)
        {
            for (int i = 15; i <= 22; i++)
            {
                var testDate = new DateTime(year, 10, i, 0, 0, 0);
                if (testDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    return testDate;
                }
            }

            return default;
        }

        public static DateTime GetTexasStateFairStartDate(this int year)
        {
            // last friday in september

            for (int i = 30; i >= 20; i--)
            {
                var testDate = new DateTime(year, 9, i, 0, 0, 0);
                if (testDate.DayOfWeek == DayOfWeek.Friday)
                {
                    return testDate;
                }
            }

            return default;
        }
        #endregion
    }
}

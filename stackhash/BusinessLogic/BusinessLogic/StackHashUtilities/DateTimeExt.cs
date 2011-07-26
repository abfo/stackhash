using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StackHashUtilities
{
    public static class DateTimeExtensions
    {
        public static DateTime RoundToPreviousSecond(this DateTime originalDateTime)
        {
            return new DateTime(originalDateTime.Year, originalDateTime.Month, originalDateTime.Day,
                originalDateTime.Hour, originalDateTime.Minute, originalDateTime.Second, originalDateTime.Kind);
        }

        public static DateTime RoundToNextSecond(this DateTime originalDateTime)
        {
            DateTime result = originalDateTime.AddSeconds(1);
            result = new DateTime(result.Year, result.Month, result.Day,
                result.Hour, result.Minute, result.Second, result.Kind);
            return result;
        }

        public static DateTime RoundToPreviousMinute(this DateTime originalDateTime)
        {
            return new DateTime(originalDateTime.Year, originalDateTime.Month, originalDateTime.Day,
                originalDateTime.Hour, originalDateTime.Minute, 0, originalDateTime.Kind);
        }

        public static DateTime RoundToNextMinute(this DateTime originalDateTime)
        {
            DateTime result = originalDateTime.AddMinutes(1);
            result = new DateTime(result.Year, result.Month, result.Day,
                 result.Hour, result.Minute + 1, 0, result.Kind);
            return result;
        }
    }
}

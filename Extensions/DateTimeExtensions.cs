using System;

namespace VkToTg.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToShortDateTimeString(this DateTime dateTime)
        {
            string output = $"{dateTime.ToShortTimeString()}";
            if (dateTime.DayOfYear != DateTime.Now.DayOfYear)
                output = $"{dateTime.ToShortDateString()} {output}";

            return output;
        }
    }
}

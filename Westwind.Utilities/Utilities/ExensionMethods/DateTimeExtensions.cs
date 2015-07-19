using System;

namespace Westwind.Utilities.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Returns true if the date is between or equal to one of the two values.
        /// </summary>
        /// <param name="date">DateTime Base, from where the calculation will be preformed.</param>
        /// <param name="startvalue">Start date to check for</param>
        /// <param name="endvalue">End date to check for</param>
        /// <returns>boolean value indicating if the date is between or equal to one of the two values</returns>
        public static bool Between(this DateTime date, DateTime startDate, DateTime endDate)
        {
            var ticks = date.Ticks;
            return ticks >= startDate.Ticks && ticks <= endDate.Ticks;
        }


        /// <summary>
        /// Returns 12:59:59pm time for the date passed.
        /// Useful for date only search ranges end value
        /// </summary>
        /// <param name="date">Date to convert</param>
        /// <returns></returns>
        public static DateTime EndOfDay(this DateTime date)
        {
            return date.Date.AddDays(1).AddMilliseconds(-1);
        }

        /// <summary>
        /// Returns 12:00am time for the date passed.
        /// Useful for date only search ranges start value
        /// </summary>
        /// <param name="date">Date to convert</param>
        /// <returns></returns>
        public static DateTime BeginningOfDay(this DateTime date)
        {
            return date.Date;
        }

        /// <summary>
        /// Returns the very end of the given month (the last millisecond of the last hour for the given date)
        /// </summary>
        /// <param name="obj">DateTime Base, from where the calculation will be preformed.</param>
        /// <returns>Returns the very end of the given month (the last millisecond of the last hour for the given date)</returns>
        public static DateTime EndOfMonth(this DateTime obj)
        {
            return new DateTime(obj.Year, obj.Month, DateTime.DaysInMonth(obj.Year, obj.Month), 23, 59, 59, 999);
        }

        /// <summary>
        /// Returns the Start of the given month (the fist millisecond of the given date)
        /// </summary>
        /// <param name="obj">DateTime Base, from where the calculation will be preformed.</param>
        /// <returns>Returns the Start of the given month (the fist millisecond of the given date)</returns>
        public static DateTime BeginningOfMonth(this DateTime obj)
        {
            return new DateTime(obj.Year, obj.Month, 1, 0, 0, 0, 0);
        }

    }
}

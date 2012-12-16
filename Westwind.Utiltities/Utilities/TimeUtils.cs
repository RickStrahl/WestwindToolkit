#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2008 - 2009
 *          http://www.west-wind.com/
 * 
 * Created: 09/08/2008
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.Globalization;

namespace Westwind.Utilities
{
    /// <summary>
    /// Time Utilities class provides date and time related routines.
    /// </summary>
    public static class TimeUtils
    {

        public static DateTime MIN_DATE_VALUE = new DateTime(1900, 1, 1, 0, 0, 0, 0, CultureInfo.InvariantCulture.Calendar, DateTimeKind.Utc);

        /// <summary>
        /// Displays a date in friendly format.
        /// </summary>
        /// <param name="Date"></param>
        /// <param name="ShowTime"></param>
        /// <returns>Today,Yesterday,Day of week or a string day (Jul 15, 2008)</returns>
        public static string FriendlyDateString(DateTime Date, bool ShowTime)
        {
            if (Date < TimeUtils.MIN_DATE_VALUE)
                return string.Empty;

            string FormattedDate = string.Empty;
            if (Date.Date == DateTime.Today)
                FormattedDate = "Today"; //Resources.Resources.Today; 
            else if (Date.Date == DateTime.Today.AddDays(-1))
                FormattedDate = "Yesterday"; //Resources.Resources.Yesterday;
            else if (Date.Date > DateTime.Today.AddDays(-6))
                // Show the Day of the week
                FormattedDate = Date.ToString("dddd").ToString();
            else
                FormattedDate = Date.ToString("MMMM dd, yyyy");

            if (ShowTime)
                FormattedDate += " @ " + Date.ToString("t").ToLower().Replace(" ","");

            return FormattedDate;
        }


        /// <summary>
        /// Returns a short date time string 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="showTime"></param>
        /// <returns></returns>
        public static string ShortDateString(DateTime date, bool showTime=false)
        {
            if (date < TimeUtils.MIN_DATE_VALUE)
                return string.Empty;

            string dateString = date.ToString("MMM dd, yyyy");
            if (!showTime)
                return dateString;

            return dateString + " - " + date.ToString("h:mmtt").ToLower();
        }

        /// <summary>
        /// Returns a short date time string 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="ShowTime"></param>
        /// <returns></returns>
        public static string ShortDateString(DateTime? date, bool ShowTime)
        {
            if (date == null || !date.HasValue)
                return string.Empty;

            return ShortDateString(date.Value, ShowTime);
        }

        /// <summary>
        /// Displays a number of milliseconds as friendly seconds, hours, minutes 
        /// Pass -1 to get a blank date.
        /// </summary>
        /// <param name="milliSeconds">the elapsed milliseconds to display time for</param>           
        /// <returns>string in format of just now or 1m ago, 2h ago</returns>
        public static string FriendlyElapsedTimeString(int milliSeconds)
        {
            if (milliSeconds < 0)
                return string.Empty;

            if (milliSeconds < 60000)
                return "just now";

            if (milliSeconds < 3600000)
                return ((int)(milliSeconds / 60000)).ToString() + "m ago";

            return ((int)(milliSeconds / 3600000)).ToString() + "h ago";
        }

        /// <summary>
        /// Displays the elapsed time  friendly seconds, hours, minutes 
        /// </summary>
        /// <param name="elapsed">Timespan of elapsed time</param>
        /// <returns>string in format of just now or 1m ago, 2h ago</returns>
        public static string FriendlyElapsedTimeString(TimeSpan elapsed)
        {
            return FriendlyElapsedTimeString((int)elapsed.TotalMilliseconds);
        }

        /// <summary>
        /// Converts a fractional hour value like 1.25 to 1:15  hours:minutes format
        /// </summary>
        /// <param name="hours">Decimal hour value</param>
        /// <param name="format">An optional format string where {0} is hours and {1} is minutes (ie: "{0}h:{1}m").</param>
        /// <returns></returns>
        public static string FractionalHoursToString(decimal hours, string format)
        {
            if (string.IsNullOrEmpty(format))
                format = "{0}:{1}";

            TimeSpan tspan = TimeSpan.FromHours((double)hours);

            // Account for rounding error
            int minutes = tspan.Minutes;
            if (tspan.Seconds > 29)
                minutes++;

            return string.Format(format, tspan.Hours + tspan.Days * 24, minutes);
        }
        /// <summary>
        /// Converts a fractional hour value like 1.25 to 1:15  hours:minutes format
        /// </summary>
        /// <param name="hours">Decimal hour value</param>
        public static string FractionalHoursToString(decimal hours)
        {
            return FractionalHoursToString(hours, null);
        }

        /// <summary>
        /// Rounds an hours value to a minute interval
        /// 0 means no rounding
        /// </summary>
        /// <param name="minuteInterval">Minutes to round up or down to</param>
        /// <returns></returns>
        public static decimal RoundDateToMinuteInterval(decimal hours, int minuteInterval,
                                                        RoundingDirection direction)
        {
            if (minuteInterval == 0)
                return hours;

            decimal fraction = 60 / minuteInterval;

            switch (direction)
            {
                case RoundingDirection.Round:
                    return Math.Round(hours * fraction, 0) / fraction;
                case RoundingDirection.RoundDown:
                    return Math.Truncate(hours * fraction) / fraction;

            }
            return Math.Ceiling(hours * fraction) / fraction;
        }

        /// <summary>
        /// Rounds a date value to a given minute interval
        /// </summary>
        /// <param name="time">Original time value</param>
        /// <param name="minuteInterval">Number of minutes to round up or down to</param>
        /// <returns></returns>
        public static DateTime RoundDateToMinuteInterval(DateTime time, int minuteInterval,
                                                         RoundingDirection direction)
        {
            if (minuteInterval == 0)
                return time;

            decimal interval = (decimal)minuteInterval;
            decimal actMinute = (decimal)time.Minute;

            if (actMinute == 0.00M)
                return time;

            int newMinutes = 0;

            switch (direction)
            {
                case RoundingDirection.Round:
                    newMinutes = (int)(Math.Round(actMinute / interval, 0) * interval);
                    break;
                case RoundingDirection.RoundDown:
                    newMinutes = (int)(Math.Truncate(actMinute / interval) * interval);
                    break;
                case RoundingDirection.RoundUp:
                    newMinutes = (int)(Math.Ceiling(actMinute / interval) * interval);
                    break;
            }

            // strip time 
            time = time.AddMinutes(time.Minute * -1);
            time = time.AddSeconds(time.Second * -1);
            time = time.AddMilliseconds(time.Millisecond * -1);

            // add new minutes back on            
            return time.AddMinutes(newMinutes);
        }

        /// <summary>
        /// Creates a DateTime value from date and time input values
        /// </summary>
        /// <param name="Date"></param>
        /// <param name="Time"></param>
        /// <returns></returns>
        public static DateTime DateTimeFromDateAndTime(string Date, string Time)
        {
            return DateTime.Parse(Date + " " + Time);
        }

        /// <summary>
        /// Creates a DateTime Value from a DateTime date and a string time value.
        /// </summary>
        /// <param name="Date"></param>
        /// <param name="Time"></param>
        /// <returns></returns>
        public static DateTime DateTimeFromDateAndTime(DateTime Date, string Time)
        {
            return DateTime.Parse(Date.ToShortDateString() + " " + Time);
        }

        /// <summary>
        /// Converts the passed date time value to Mime formatted time string
        /// </summary>
        /// <param name="Time"></param>
        public static string MimeDateTime(DateTime Time)
        {
            TimeSpan Offset = TimeZone.CurrentTimeZone.GetUtcOffset(Time);

            string sOffset = null;
            if (Offset.Hours < 0)
                sOffset = "-" + (Offset.Hours * -1).ToString().PadLeft(2, '0');
            else
                sOffset = "+" + Offset.Hours.ToString().PadLeft(2, '0');

            sOffset += Offset.Minutes.ToString().PadLeft(2, '0');

            return "Date: " + Time.ToString("ddd, dd MMM yyyy HH:mm:ss",
                                                          System.Globalization.CultureInfo.InvariantCulture) +
                                                          " " + sOffset;
        }

    }

    /// <summary>
    /// Determines how date time values are rounded
    /// </summary>
    public enum RoundingDirection
    {
        RoundUp,
        RoundDown,
        Round
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Westwind.Utilities;
using System.Collections;
using System.IO;

namespace Westwind.Web.Mvc
{
    /// <summary>
    /// Set of generic MVC Html Helper utilities
    /// </summary>
    public static class MvcHtmlUtils
    {

        /// <summary>
        /// Returns a selectListItem list for an enumerated value
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> SelectListItemsFromEnum(Type enumType, bool valueAsFieldValueNumber = false)
        {
            var vals = ReflectionUtils.GetEnumList(enumType,valueAsFieldValueNumber);            

            foreach (var val in vals)
            {
                yield return  new SelectListItem() 
                { 
                    Value = val.Key,
                    Text = val.Value 
                };                
            }
        }

        /// <summary>
        /// Takes a dictionary of values and turns it into a SelectListItems enumeration.
        /// Converts the dictionary keys  into the value and the dictionary value into
        /// the text.
        /// </summary>
        /// <remarks>
        /// Assumes the dictionary keys and values can be turned 
        /// into strings using ToString()
        /// </remarks>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> SelectListItemsFromDictionary(IDictionary dict)
        {
            foreach (DictionaryEntry entry in dict)
            {
                string text = entry.Value.ToString();
                string val = entry.Key.ToString();

                yield return new SelectListItem()
                {
                     Text = text,
                     Value = val
                };
            }
        }

        /// <summary>
        /// Returns a select list of months of the year set with 01-12 and
        /// a custom format display string (01 - January by default)
        /// </summary>
        /// <param name="formatString">Optional format string to format the month string. Based on a Date variable</param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> SelectListMonths(string formatString = "MM - MMMM")
        {
            var monthList = new SelectListItem[12];
            for (int i = 0; i < 12; i++)
            {
                var date = new DateTime(2000, i + 1, 1);
                var number = (i + 1).ToString().PadLeft(2, '0');
                var month = date.ToString(formatString);
                monthList[i] = new SelectListItem
                {
                    Value = number,
                    Text = month
                };
            }
            return monthList;
        }

        /// <summary>
        /// Gets a select list of years.
        /// </summary>
        /// <param name="numberOfYears">Number of years to display</param>
        /// <param name="startYear">Year to start. If 0 starts current year</param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> SelectListYears(int numberOfYears = 10, int startYear = 0)
        {
            if (startYear == 0)
                startYear = DateTime.Now.Year; 

            var yearList = new SelectListItem[numberOfYears];
            for (int i = 0; i < numberOfYears; i++)
            {
                

                string dt = (startYear + i).ToString();
                yearList[i] = new SelectListItem
                {
                    Value = dt,
                    Text = dt                    
                };
            }

            return yearList;
        }


    }
}

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
            foreach (var entry in dict.Keys)
            {
                yield return new SelectListItem()
                {
                     Text = dict[entry].ToString(),
                     Value = entry.ToString()                    
                };
            }
        }


    }
}

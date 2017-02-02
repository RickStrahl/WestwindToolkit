using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Westwind.Utilities
{
    public static class GenericUtils
    {
        /// <summary>
        /// Determines whether an item is contained in a list of other items
        /// </summary>
        /// <example>
        /// bool exists = Inlist&lt;string&gt;("Rick","Mike","Billy","Rick","Frank"); // true;
        /// </example>
        /// <typeparam name="T">Any type</typeparam>
        /// <param name="item">The item to look for</param>
        /// <param name="list">Any number of items to search (params)</param>
        /// <returns></returns>
        public static bool Inlist<T>(T item, params T[] list)
        {
            return list.Contains(item);
        }
    }
}

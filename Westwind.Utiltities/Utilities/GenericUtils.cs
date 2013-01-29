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
        /// <typeparam name="T">Any type</typeparam>
        /// <param name="item"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool Inlist<T>(T item, params T[] list)
        {
            return list.Contains(item);
        }
    }
}

using System;

namespace Westwind.Utilities.Extensions
{
    public static class GuidExtensions
    {
        /// <summary>
        /// Determines whether this is an empty guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static bool IsEmpty(this Guid guid)
        {
            if (guid == Guid.Empty)
                return true;
            return false;
        }
    }
}

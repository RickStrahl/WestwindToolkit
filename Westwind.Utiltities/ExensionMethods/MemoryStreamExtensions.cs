using System.Text;
using System.IO;

namespace Westwind.Utilities.Extensions
{
    /// <summary>
    /// MemoryStream Extension Methods that provide conversions to and from strings
    /// </summary>
    public static class MemoryStreamExtensions
    {
        /// <summary>
        /// Returns the content of the stream as a string
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string GetAsString(this MemoryStream ms, Encoding encoding)
        {
            return encoding.GetString(ms.ToArray());
        }


        /// <summary>
        /// Returns the content of the stream as a string
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static string GetAsString(this MemoryStream ms)
        {
            return GetAsString(ms, Encoding.Default);
        }

        /// <summary>
        /// Writes the specified string into the memory stream
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="inputString"></param>
        /// <param name="encoding"></param>
        public static void WriteString(this MemoryStream ms, string inputString, Encoding encoding)
        {
            byte[] buffer = encoding.GetBytes(inputString);
            ms.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes the specified string into the memory stream
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="inputString"></param>
        public static void WriteString(this MemoryStream ms, string inputString)
        {
            WriteString(ms, inputString, Encoding.Default);
        }      
    }
}

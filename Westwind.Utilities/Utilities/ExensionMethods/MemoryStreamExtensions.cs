using System.Text;
using System.IO;

namespace System.IO
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
        public static string AsString(this MemoryStream ms, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.Unicode;

            return encoding.GetString(ms.ToArray());
        }

        /// <summary>
        /// Writes the specified string into the memory stream
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="inputString"></param>
        /// <param name="encoding"></param>
        public static void FromString(this MemoryStream ms, string inputString, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.Unicode;

            byte[] buffer = encoding.GetBytes(inputString);
            ms.Write(buffer, 0, buffer.Length);
        }
    }
}

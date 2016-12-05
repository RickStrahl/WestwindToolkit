#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2009
 *          http://www.west-wind.com/
 * 
 * Created: 09/12/2009
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Westwind.Utilities
{
	/// <summary>
	/// wwUtils class which contains a set of common utility classes for 
	/// Formatting strings
	/// Reflection Helpers
	/// Object Serialization
    /// Stream Manipulation
	/// </summary>
	public static class FileUtils
	{

        /// <summary>
        /// Copies the content of the one stream to another.
        /// Streams must be open and stay open.
        /// </summary>
        public static void CopyStream(Stream source, Stream dest, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int read;
            while ( (read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                dest.Write(buffer, 0, read);
            }
        }

        /// <summary>
        /// Copies the content of one stream to another by appending to the target stream
        /// Streams must be open when passed in.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="bufferSize"></param>
        /// <param name="append"></param>
        public static void CopyStream(Stream source, Stream dest, int bufferSize, bool append)
        {
            if (append)
                dest.Seek(0, SeekOrigin.End);

            CopyStream(source, dest, bufferSize);
            return;
        }

        /// <summary>
        /// Detects the byte order mark of a file and returns
        /// an appropriate encoding for the file.
        /// </summary>
        /// <param name="srcFile"></param>
        /// <returns></returns>
        public static Encoding GetFileEncoding(string srcFile)
        {
            // Use Default of Encoding.Default (Ansi CodePage)
            Encoding enc = Encoding.Default;

            // Detect byte order mark if any - otherwise assume default

            byte[] buffer = new byte[5];
            FileStream file = new FileStream(srcFile, FileMode.Open);
            file.Read(buffer, 0, 5);
            file.Close();

            if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
               enc = Encoding.UTF8;
            else if (buffer[0] == 0xfe && buffer[1] == 0xff)
                enc = Encoding.Unicode;
            else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
                enc = Encoding.UTF32;

            else if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
                enc = Encoding.UTF7;

            return enc;
        }
        

        /// <summary>
        /// Opens a stream reader with the appropriate text encoding applied.
        /// </summary>
        /// <param name="srcFile"></param>
        public static StreamReader OpenStreamReaderWithEncoding(string srcFile)
        {
            Encoding enc = GetFileEncoding(srcFile);
            return new StreamReader(srcFile, enc);
        }

        /// <summary>
        /// Creates a safe file and directory name that is stripped of all invalid characters.
        /// </summary>
        /// <param name="fileName">Filename to clean up</param>
        /// <param name="replace">Replacement character for invalid characters</param>
        /// <returns></returns>
        public  static string SafeFilename(string fileName, string replace = "")
        {
            return Path.GetInvalidFileNameChars()
                .Aggregate(fileName, 
                           (file, c) => file.Replace(c.ToString(), replace));
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint GetLongPathName(string ShortPath, StringBuilder sb, int buffer);

        /// <summary>
        /// This function returns the actual filename of a file
        /// that exists on disk. If you provide a path/file name
        /// that is not proper cased as input, this function fixes
        /// it up and returns the file using the path and file names
        /// as they exist on disk.
        /// 
        /// If the file doesn't exist the original filename is 
        /// returned.
        /// </summary>
        /// <param name="filename">A filename to check</param>
        /// <returns>On disk file name and path with the disk casing</returns>
	    public static string GetPhysicalPath(string filename)
	    {
	        try
	        {
	            StringBuilder sb = new StringBuilder(1500);
	            uint result = GetLongPathName(filename, sb, sb.Capacity);
	            if (result > 0)
	                filename = sb.ToString();
	        }
            catch { }

            return filename;
        }

        /// <summary>
        /// Returns the full path of a full physical filename
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string JustPath(string path) 
		{
			FileInfo fi = new FileInfo(path);
			return fi.DirectoryName + "\\";
		}

        /// <summary>
        /// Returns a fully qualified path from a partial or relative
        /// path.
        /// </summary>
        /// <param name="Path"></param>
        public static string GetFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            return Path.GetFullPath(path);
        }

		/// <summary>
		/// Returns a relative path string from a full path based on a base path
        /// provided.
		/// </summary>
		/// <param name="fullPath">The path to convert. Can be either a file or a directory</param>
		/// <param name="basePath">The base path on which relative processing is based. Should be a directory.</param>
		/// <returns>
		/// String of the relative path.
		/// 
		/// Examples of returned values:
		///  test.txt, ..\test.txt, ..\..\..\test.txt, ., .., subdir\test.txt
		/// </returns>
		public static string GetRelativePath(string fullPath, string basePath ) 
		{
            // ForceBasePath to a path
            if (!basePath.EndsWith("\\"))
                basePath += "\\";

            Uri baseUri = new Uri(basePath);
            Uri fullUri = new Uri(fullPath);

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);

            // Uri's use forward slashes so convert back to backward slahes
            return relativeUri.ToString().Replace("/", "\\");


            //// Start by normalizing paths
            //fullPath = fullPath.ToLower();
            //basePath = basePath.ToLower();

            //if ( basePath.EndsWith("\\") ) 
            //    basePath = basePath.Substring(0,basePath.Length-1);
            //if ( fullPath.EndsWith("\\") ) 
            //    fullPath = fullPath.Substring(0,fullPath.Length-1);

            //// First check for full path
            //if ( (fullPath+"\\").IndexOf(basePath + "\\") > -1) 
            //    return  fullPath.Replace(basePath,".");

            //// Now parse backwards
            //string BackDirs = string.Empty;
            //string PartialPath = basePath;
            //int Index = PartialPath.LastIndexOf("\\");
            //while (Index > 0) 
            //{
            //    // Strip path step string to last backslash
            //    PartialPath = PartialPath.Substring(0,Index );
			
            //    // Add another step backwards to our pass replacement
            //    BackDirs = BackDirs + "..\\" ;

            //    // Check for a matching path
            //    if ( fullPath.IndexOf(PartialPath) > -1 ) 
            //    {
            //        if ( fullPath == PartialPath )
            //            // We're dealing with a full Directory match and need to replace it all
            //            return fullPath.Replace(PartialPath,BackDirs.Substring(0,BackDirs.Length-1) );
            //        else
            //            // We're dealing with a file or a start path
            //            return fullPath.Replace(PartialPath+ (fullPath == PartialPath ?  string.Empty : "\\"),BackDirs);
            //    }
            //    Index = PartialPath.LastIndexOf("\\",PartialPath.Length-1);
            //}

            //return fullPath;
		}

        /// <summary>
        /// Deletes files based on a file spec and a given timeout.
        /// This routine is useful for cleaning up temp files in 
        /// Web applications.
        /// </summary>
        /// <param name="filespec">A filespec that includes path and/or wildcards to select files</param>
        /// <param name="seconds">The timeout - if files are older than this timeout they are deleted</param>
        public static void DeleteTimedoutFiles(string filespec,int seconds)
        {
            string path = Path.GetDirectoryName(filespec);
            string spec = Path.GetFileName(filespec);
            string[] files = Directory.GetFiles(path,spec);

            foreach(string file in files)
            {
                try
                {
                    if (File.GetLastWriteTimeUtc(file) < DateTime.UtcNow.AddSeconds(seconds * -1))
                        File.Delete(file);
                }
                catch {}  // ignore locked files
            }
        }
            
		
    }

}
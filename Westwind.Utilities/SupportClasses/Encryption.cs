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
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.IO.Compression;


namespace Westwind.Utilities
{
	/// <summary>
	/// A simple encryption class that can be used to two-way encode/decode strings and byte buffers
	/// with single method calls.
	/// </summary>
	public class Encryption
	{
		/// <summary>
		/// Replace this value with some unique key of your own
		/// Best set this in your App start up in a Static constructor
		/// </summary>
		public static string Key = "0a1f131c";

		/// <summary>
		/// Encodes a stream of bytes using DES encryption with a pass key. Lowest level method that 
		/// handles all work.
		/// </summary>
		/// <param name="InputString"></param>
		/// <param name="EncryptionKey"></param>
		/// <returns></returns>
		public static byte[] EncryptBytes(byte[] InputString, string EncryptionKey) 
		{
			if (EncryptionKey == null)
				EncryptionKey = Key;

			TripleDESCryptoServiceProvider des =  new TripleDESCryptoServiceProvider();
			MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();

			des.Key = hashmd5.ComputeHash(Encoding.ASCII.GetBytes(EncryptionKey));
			des.Mode = CipherMode.ECB;
			
			ICryptoTransform Transform = des.CreateEncryptor();

			byte[] Buffer = InputString;
			return Transform.TransformFinalBlock(Buffer,0,Buffer.Length);
		}
		
		/// <summary>
		/// Encrypts a string into bytes using DES encryption with a Passkey. 
		/// </summary>
		/// <param name="InputString"></param>
		/// <param name="EncryptionKey"></param>
		/// <returns></returns>
		public static byte[] EncryptBytes(string DecryptString, string EncryptionKey) 
		{
			return EncryptBytes(Encoding.ASCII.GetBytes(DecryptString),EncryptionKey);
		}

		/// <summary>
		/// Encrypts a string using Triple DES encryption with a two way encryption key.String is returned as Base64 encoded value
		/// rather than binary.
		/// </summary>
		/// <param name="InputString"></param>
		/// <param name="EncryptionKey"></param>
		/// <returns></returns>
		public static string EncryptString(string InputString, string EncryptionKey) 
		{
			return Convert.ToBase64String( EncryptBytes(Encoding.ASCII.GetBytes(InputString),EncryptionKey) );
		}

		
		/// <summary>
		/// Decrypts a Byte array from DES with an Encryption Key.
		/// </summary>
		/// <param name="DecryptBuffer"></param>
		/// <param name="EncryptionKey"></param>
		/// <returns></returns>
		public static byte[] DecryptBytes(byte[] DecryptBuffer, string EncryptionKey) 
		{
            if (DecryptBuffer == null || DecryptBuffer.Length == 0)
                return null;

			if (EncryptionKey == null)
				EncryptionKey = Key;

			TripleDESCryptoServiceProvider des =  new TripleDESCryptoServiceProvider();
			MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();

			des.Key = hashmd5.ComputeHash(Encoding.ASCII.GetBytes(EncryptionKey));
			des.Mode = CipherMode.ECB;

			ICryptoTransform Transform = des.CreateDecryptor();
			
			return  Transform.TransformFinalBlock(DecryptBuffer,0,DecryptBuffer.Length);
		}
		
		public static byte[] DecryptBytes(string DecryptString, string EncryptionKey) 
		{	
				return DecryptBytes(Convert.FromBase64String(DecryptString),EncryptionKey);
		}

		/// <summary>
		/// Decrypts a string using DES encryption and a pass key that was used for 
		/// encryption.
		/// <seealso>Class wwEncrypt</seealso>
		/// </summary>
		/// <param name="DecryptString"></param>
		/// <param name="EncryptionKey"></param>
		/// <returns>String</returns>
		public static string DecryptString(string DecryptString, string EncryptionKey) 
		{
			try 
			{
				return Encoding.ASCII.GetString( DecryptBytes(Convert.FromBase64String(DecryptString),EncryptionKey));
			}
			catch { return string.Empty; }  // Probably not encoded
        }


        /// <summary>
        /// Generates a hash for the given plain text value and returns a
        /// base64-encoded result. Before the hash is computed, a random salt
        /// is generated and appended to the plain text. This salt is stored at
        /// the end of the hash value, so it can be used later for hash
        /// verification.
        /// </summary>
        /// <param name="plainText">
        /// Plaintext value to be hashed. 
        /// </param>
        /// <param name="hashAlgorithm">
        /// Name of the hash algorithm. Allowed values are: "MD5", "SHA1",
        /// "SHA256", "SHA384", and "SHA512" (if any other value is specified
        /// MD5 hashing algorithm will be used). This value is case-insensitive.
        /// </param>
        /// <param name="saltBytes">
        /// Salt bytes. This parameter can be null, in which case a random salt
        /// value will be generated.
        /// </param>
        /// <returns>
        /// Hash value formatted as a base64-encoded string.
        /// </returns>
        /// <remarks>
        /// ComputeHash code provided as an example by Obviex at
        /// http://www.obviex.com/samples/hash.aspx
        /// As noted by Obviex themselves, code is definitely not optimally efficient.
        /// Should performance requirements necessitate improvement, this should
        /// be improved.
        /// </remarks>
        public static string ComputeHash(string plainText,
                                         string hashAlgorithm,
                                         byte[] saltBytes)
        {
            if (plainText == null)
                return null;
            
            // If salt is not specified, generate it on the fly.
            if (saltBytes == null)
            {
                // Define min and max salt sizes.
                int minSaltSize = 4;
                int maxSaltSize = 8;

                // Generate a random number for the size of the salt.
                Random random = new Random();
                int saltSize = random.Next(minSaltSize, maxSaltSize);

                // Allocate a byte array, which will hold the salt.
                saltBytes = new byte[saltSize];

                // Initialize a random number generator.
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

                // Fill the salt with cryptographically strong byte values.
                rng.GetNonZeroBytes(saltBytes);
            }

            // Convert plain text into a byte array.
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            // Allocate array, which will hold plain text and salt.
            byte[] plainTextWithSaltBytes =
                    new byte[plainTextBytes.Length + saltBytes.Length];

            // Copy plain text bytes into resulting array.
            for (int i = 0; i < plainTextBytes.Length; i++)
                plainTextWithSaltBytes[i] = plainTextBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < saltBytes.Length; i++)
                plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];

            // Because we support multiple hashing algorithms, we must define
            // hash object as a common (abstract) base class. We will specify the
            // actual hashing algorithm class later during object creation.
            HashAlgorithm hash;

            // Make sure hashing algorithm name is specified.
            if (hashAlgorithm == null)
                hashAlgorithm = "";

            // Initialize appropriate hashing algorithm class.
            switch (hashAlgorithm.ToUpper())
            {
                case "SHA1":
                    hash = new SHA1Managed();
                    break;

                case "SHA256":
                    hash = new SHA256Managed();
                    break;

                case "SHA384":
                    hash = new SHA384Managed();
                    break;

                case "SHA512":
                    hash = new SHA512Managed();
                    break;

                default:
                    hash = new MD5CryptoServiceProvider();
                    break;
            }

            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

            // Create array which will hold hash and original salt bytes.
            byte[] hashWithSaltBytes = new byte[hashBytes.Length +
                                                saltBytes.Length];

            // Copy hash bytes into resulting array.
            for (int i = 0; i < hashBytes.Length; i++)
                hashWithSaltBytes[i] = hashBytes[i];

            // Append salt bytes to the result.
            for (int i = 0; i < saltBytes.Length; i++)
                hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];

            // Convert result into a base64-encoded string.
            string hashValue = Convert.ToBase64String(hashWithSaltBytes);

            // Return the result.
            return hashValue;
        }


        
        /// <summary>
        /// GZip encodes a memory buffer to a compressed memory buffer
        /// </summary>
        /// <param name="Buffer"></param>
        /// <returns></returns>
        public static byte[] GZipMemory(byte[] Buffer)
        {
            MemoryStream ms = new MemoryStream();

            GZipStream GZip = new GZipStream(ms, CompressionMode.Compress);

            GZip.Write(Buffer, 0, Buffer.Length);
            GZip.Close();

            byte[] Result = ms.ToArray();
            ms.Close();

            return Result;
        }

        /// <summary>
        /// Encodes a string to a gzip compressed memory buffer
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static byte[] GZipMemory(string Input)
        {
            return GZipMemory(Encoding.ASCII.GetBytes(Input));
        }

        /// <summary>
        /// Encodes a file to a gzip memory buffer
        /// </summary>
        /// <param name="Filename"></param>
        /// <param name="IsFile"></param>
        /// <returns></returns>
        public static byte[] GZipMemory(string Filename, bool IsFile)
        {
            string InputFile = Filename;
            byte[] Buffer = File.ReadAllBytes(Filename);
            return GZipMemory(Buffer);
        }

        /// <summary>
        /// Encodes one file to another file that is gzip compressed.
        /// File is overwritten if it exists and not locked.
        /// </summary>
        /// <param name="Filename"></param>
        /// <param name="OutputFile"></param>
        /// <returns></returns>
        public static bool GZipFile(string Filename, string OutputFile)
        {
            string InputFile = Filename;
            byte[] Buffer = File.ReadAllBytes(Filename);
            FileStream fs = new FileStream(OutputFile, FileMode.OpenOrCreate, FileAccess.Write);
            GZipStream GZip = new GZipStream(fs, CompressionMode.Compress);
            GZip.Write(Buffer, 0, Buffer.Length);
            GZip.Close();
            fs.Close();

            return true;
        }
        
    }
}

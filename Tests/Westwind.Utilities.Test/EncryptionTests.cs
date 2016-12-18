using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Westwind.Utilities.Test
{
    [TestClass]
    public class EncryptionTests
    {

        [TestMethod]
        public void EncryptDecryptString()
        {
            string data = "Seekrit!Password";
            string key = "my+keeper";

            string encrypted = Encryption.EncryptString(data,key);
            string decrypted = Encryption.DecryptString(encrypted,key);

            Assert.AreNotEqual(data, encrypted);
            Assert.AreEqual(data, decrypted);

            Console.WriteLine(encrypted);            
        }

        [TestMethod]
        public void EncryptDecryptWithExtendedCharacterString()
        {            
            string data = "Seekrit°!Password";
            string key = "my+keeper";

            string encrypted = Encryption.EncryptString(data, key);
            string decrypted = Encryption.DecryptString(encrypted, key);

            Assert.AreNotEqual(data, encrypted);
            Assert.AreEqual(data, decrypted);

            Console.WriteLine(encrypted);
        }


        [TestMethod]
        public void EncryptDecryptWithExtendedCharacterStringByteKey()
        {
            string data = "Seekrit°!Password";
            byte[] key = new byte[] {10, 20, 88, 223, 132, 1, 55, 32};

            string encrypted = Encryption.EncryptString(data, key);
            string decrypted = Encryption.DecryptString(encrypted, key);

            Assert.AreNotEqual(data, encrypted);
            Assert.AreEqual(data, decrypted);

            Console.WriteLine(encrypted);
        }

        [TestMethod]
        public void EncryptDecryptWithExtendedCharacterByteData()
        {
            byte[] data = new byte[] {1, 3, 22, 224, 113, 53, 31, 6, 12, 44, 49, 66};
            byte[] key = new byte[] { 2, 3, 4, 5, 6};

            byte[] encrypted = Encryption.EncryptBytes(data, key);
            byte[] decrypted = Encryption.DecryptBytes(encrypted, key);

            Assert.IsTrue(decrypted.SequenceEqual(data));

            Console.WriteLine(encrypted);
        }


        [TestMethod]
        public void HashValues()
        {             
            string data = "Seekrit!Password";
            byte[] salt = new byte[] { 10, 22, 144, 51, 55, 61};
            string algo = "SHA1";

            string encrypted = Encryption.ComputeHash(data, algo, salt,useBinHex: true);
            Console.WriteLine(encrypted);

            data = "test";
            encrypted = Encryption.ComputeHash(data, algo, salt, useBinHex: true);
            Console.WriteLine(encrypted);

            data = "testa";
            encrypted = Encryption.ComputeHash(data, algo, salt, useBinHex: true);
            Console.WriteLine(encrypted);

            data = "t";
            encrypted = Encryption.ComputeHash(data, algo, salt, useBinHex: true);
            Console.WriteLine(encrypted);
        }




    }
}

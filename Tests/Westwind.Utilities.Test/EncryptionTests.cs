using System;
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

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
            

            data = "t";
            encrypted = Encryption.ComputeHash(data, algo, salt, useBinHex: true);
            Console.WriteLine(encrypted);


            data = "testa";
            encrypted = Encryption.ComputeHash(data, algo, salt, useBinHex: true);
            Console.WriteLine(encrypted);

            data = "testa";
            var encrypted2 = Encryption.ComputeHash(data, algo, salt, useBinHex: true);
            Console.WriteLine(encrypted);


        }


        [TestMethod]
        public void HashValuesHMAC()
        {
            string data = "Seekrit!Password";
            byte[] salt = new byte[] { 10, 22, 144, 51, 55, 61 };
            string saltString = "bogus";
            string algo = "SHA256";

            string encrypted = Encryption.ComputeHash(data, algo, salt, useBinHex: true);
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

            var binData = new byte[] { 10, 20, 21, 44, 55, 233, 122, 193 };
            encrypted = Encryption.ComputeHash(binData, algo, salt, useBinHex: true);
            Console.WriteLine(encrypted);

            data = "test";
            encrypted = Encryption.ComputeHash(data, algo, saltString, useBinHex: true);
            Console.WriteLine(encrypted);


            data = "test";
            var encrypted2 = Encryption.ComputeHash(data, algo, saltString, useBinHex: false);
            Console.WriteLine(encrypted);

            byte[] decryptBytes1 = Encryption.BinHexToBinary(encrypted);
            byte[] decryptBytes2 = Convert.FromBase64String(encrypted2);

            Assert.IsTrue( decryptBytes1.SequenceEqual(decryptBytes2));
        }

        [TestMethod]
        public void BinHexBase64CompareTest()
        {

            string algo = "HMACSHA256";
            string saltString = "bogus";

            string data = "test";
            string encrypted = Encryption.ComputeHash(data, algo, saltString, useBinHex: true);
            Console.WriteLine(encrypted);
            
            string encrypted2 = Encryption.ComputeHash(data, algo, saltString, useBinHex: false);
            Console.WriteLine(encrypted2);
            
            byte[] decryptBytes1 = Encryption.BinHexToBinary(encrypted);
            byte[] decryptBytes2 = Convert.FromBase64String(encrypted2);

            Assert.IsTrue(decryptBytes1.SequenceEqual(decryptBytes2));
        }



    }
}

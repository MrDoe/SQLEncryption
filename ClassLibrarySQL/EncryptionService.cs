using Microsoft.SqlServer.Server;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SQLEncryption
{
    public class EncryptionService
    {
        [SqlFunction]
        public static string Encrypt(string data, string password)
        {
            // no data
            if (data == null)
                return "";

            // get hash keys from password
            byte[][] keys = GetHashKeys(password);

            string encData = null;
            try
            {
                encData = EncryptStringToBytes_Aes(data, keys[0], keys[1]);
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return encData;
        }
        
        [SqlFunction]
        public static string Decrypt(string data, string password)
        {
            // no data
            if (data == null)
            {
                return "";
            }

            // get hash keys from password
            byte[][] keys = GetHashKeys(password);

            // decrypt
            string decData;

            try
            {
                decData = DecryptStringFromBytes_Aes(data, keys[0], keys[1]);
            }
            catch (Exception)
            {
                string sReturn = "";
                return sReturn.PadRight(data.Length, '#');
            }

            return decData;
        }

        private static byte[][] GetHashKeys(string key)
        {
            byte[][] result = new byte[2][];
            Encoding enc = Encoding.UTF8;

            SHA256 sha2 = new SHA256CryptoServiceProvider();

            byte[] rawKey = enc.GetBytes(key);
            byte[] rawIV = enc.GetBytes(key);

            byte[] hashKey = sha2.ComputeHash(rawKey);
            byte[] hashIV = sha2.ComputeHash(rawIV);

            Array.Resize(ref hashIV, 16);

            result[0] = hashKey;
            result[1] = hashIV;

            return result;
        }

        //source: https://msdn.microsoft.com/de-de/library/system.security.cryptography.aes(v=vs.110).aspx
        private static string EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iV)
        {
            if (plainText == null || plainText.Length <= 0)
            {
                throw new Exception("plainText");
            }

            if (key == null || key.Length <= 0)
            {
                throw new Exception("Key");
            }

            if (iV == null || iV.Length <= 0)
            {
                throw new Exception("IV");
            }

            byte[] encrypted;

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = key;
                aesAlg.IV = iV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
            return Convert.ToBase64String(encrypted);
        }

        //source: https://msdn.microsoft.com/de-de/library/system.security.cryptography.aes(v=vs.110).aspx
        private static string DecryptStringFromBytes_Aes(string cipherTextString, byte[] key, byte[] iV)
        {
            byte[] cipherText = Convert.FromBase64String(cipherTextString);

            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new Exception("cipherText");
            }

            if (key == null || key.Length <= 0)
            {
                throw new Exception("Key");
            }

            if (iV == null || iV.Length <= 0)
            {
                throw new Exception("IV");
            }

            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}

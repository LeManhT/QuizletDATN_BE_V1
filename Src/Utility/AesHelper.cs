using System.Security.Cryptography;

namespace Quizlet_App_Server.Src.Utility
{
    public static class AesHelper
    {
        public static (byte[] EncryptedData, byte[] Key, byte[] IV) EncryptData(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return (ms.ToArray(), aes.Key, aes.IV);
                }
            }
        }

        public static byte[] EncryptData(string plainText, string key, string iv)
        {
            byte[] keyByteArr = Convert.FromBase64String(key);
            byte[] ivByteArr = Convert.FromBase64String(iv);
            using (Aes aes = Aes.Create())
            {
                using (var encryptor = aes.CreateEncryptor(keyByteArr, ivByteArr))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return ms.ToArray();
                }
            }
        }

        public static string EncryptDataToBase64(string plainText, string key, string iv)
        {
            try
            {
                return Convert.ToBase64String(EncryptData(plainText, key, iv));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return plainText; 
            }
        }

        public static string DecryptData(byte[] encryptedData, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                using (var decryptor = aes.CreateDecryptor(key, iv))
                using (var ms = new MemoryStream(encryptedData))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        public static string DecryptData(string encryptedData, string key, string iv)
        {
            byte[] encryptedDataByteArr = Convert.FromBase64String(encryptedData);
            byte[] keyByteArr = Convert.FromBase64String(key);
            byte[] ivByteArr = Convert.FromBase64String(iv);
            using (Aes aes = Aes.Create())
            {
                using (var decryptor = aes.CreateDecryptor(keyByteArr, ivByteArr))
                using (var ms = new MemoryStream(encryptedDataByteArr))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }

}

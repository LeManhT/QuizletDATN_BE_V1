using System.Security.Cryptography;

namespace Quizlet_App_Server.Src.Utility
{
    public static class RsaHelper
    {
        private static RSA rsa;

        static RsaHelper()
        {
            rsa = RSA.Create();
        }

        public static string GetPublicKey()
        {
            return Convert.ToBase64String(rsa.ExportRSAPublicKey());
        }

        public static string GetPrivateKey()
        {
            return Convert.ToBase64String(rsa.ExportRSAPrivateKey());
        }

        public static byte[] EncryptWithPublicKey(byte[] data, string publicKey)
        {
            using (RSA rsaEncrypt = RSA.Create())
            {
                rsaEncrypt.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
                return rsaEncrypt.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
            }
        }

        public static byte[] DecryptWithPrivateKey(byte[] encryptedData, string privateKey)
        {
            using (RSA rsaDecrypt = RSA.Create())
            {
                rsaDecrypt.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
                return rsaDecrypt.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);
            }
        }
    }

}

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OKRAdminService.Common
{
    public static class CryptoFunctions
    {
        #region Rijndael Encryption

        public static string EncryptRijndael(string input, string salt)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException("input");
            var aesAlg = NewRijndaelManaged(salt);
            var encryption = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryption, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(input);
            }
            return Convert.ToBase64String(msEncrypt.ToArray());
        }
        public static string DecryptRijndael(string cipherinput, string salt)
        {
            if (string.IsNullOrEmpty(cipherinput))
                throw new ArgumentNullException("cipherinput");
            if (!IsBase64String(cipherinput))
                throw new FormatException("The cipherText input parameter is not base64 encoded");
            var aesAlg = NewRijndaelManaged(salt);
            var decryption = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            var cipher = Convert.FromBase64String(cipherinput);
            using var msDecrypt = new MemoryStream(cipher);
            using var csDecrypt = new CryptoStream(msDecrypt, decryption, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            var text = srDecrypt.ReadToEnd();

            return text;
        }
        private static RijndaelManaged NewRijndaelManaged(string salt)
        {
            string InputKey = "99334E81-342C-4900-86D9-07B7B9FE5EBB";
            if (salt == null) throw new ArgumentNullException("salt");
            var saltBytes = Encoding.ASCII.GetBytes(salt);
            var key = new Rfc2898DeriveBytes(InputKey, saltBytes);

            var aesAlg = new RijndaelManaged();
            aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
            aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);
            return aesAlg;
        }
        public static bool IsBase64String(string base64String)
        {
            base64String = base64String.Trim();
            return (base64String.Length % 4 == 0) &&
                   Regex.IsMatch(base64String, AppConstants.Base64Regex, RegexOptions.None);

        }
        #endregion
    }
}


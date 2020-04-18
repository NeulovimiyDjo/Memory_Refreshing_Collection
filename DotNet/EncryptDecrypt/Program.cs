using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace EncryptDecrypt
{
    class Program
    {
        public static void Main()
        {
            var key = "encryptionKey";

            var encryptedPassword = EncryptString(key, "password");
            Console.WriteLine("Encrypted Password: " + encryptedPassword);

            var decrptedPassword = DecryptString(key, encryptedPassword);
            Console.WriteLine("Decrypted Password: " + decrptedPassword);
        }

        public static string EncryptString(string key, string input)
        {
            using (Aes aes = Aes.Create())
            {
                var hasher = new SHA256Managed();
                var hashedKey = hasher.ComputeHash(Encoding.UTF8.GetBytes(key));
                aes.Key = hashedKey;
                aes.GenerateIV();
                var iv = aes.IV;

                using (var encrypter = aes.CreateEncryptor(aes.Key, iv))
                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length); //Prepend IV to data

                    using (var cs = new CryptoStream(ms, encrypter, CryptoStreamMode.Write))
                    using (var bw = new BinaryWriter(cs))
                    {
                        var byteInput = Encoding.UTF8.GetBytes(input);
                        bw.Write(byteInput); //Encrypt data
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string DecryptString(string key, string input)
        {
            using (Aes aes = Aes.Create())
            {
                var hasher = new SHA256Managed();
                var hashedKey = hasher.ComputeHash(Encoding.UTF8.GetBytes(key));
                aes.Key = hashedKey;

                var byteInput = Convert.FromBase64String(input);

                //Get first 16 bytes of IV and use it to decrypt
                var iv = new byte[16];
                Array.Copy(byteInput, 0, iv, 0, iv.Length);

                using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
                using (var ms = new MemoryStream())
                {
                    // Get encrypted data part
                    var dataLength = byteInput.Length - iv.Length;
                    var encryptedData = new byte[dataLength];
                    Array.Copy(byteInput, iv.Length, encryptedData, 0, dataLength);

                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    using (var bw = new BinaryWriter(cs))
                    {
                        bw.Write(encryptedData); //Decrypt data
                    }

                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
    }
}

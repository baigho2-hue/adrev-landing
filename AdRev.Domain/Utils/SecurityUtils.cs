using System.Security.Cryptography;
using System.Text;

namespace AdRev.Domain.Utils;

public static class SecurityUtils
{
    private const int KeySize = 32; // 256 bit
    private const int IvSize = 16;  // 128 bit

    public static string GenerateKey()
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            var key = new byte[KeySize];
            rng.GetBytes(key);
            return Convert.ToBase64String(key);
        }
    }

    public static string Encrypt(string plainText, string keyString)
    {
        var key = Convert.FromBase64String(keyString);
        using (var aes = Aes.Create())
        {
            aes.Key = key;
            aes.GenerateIV();
            var iv = aes.IV;

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream())
            {
                // Prepend IV to ciphertext
                ms.Write(iv, 0, iv.Length);
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    public static string Decrypt(string cipherText, string keyString)
    {
        var fullCipher = Convert.FromBase64String(cipherText);
        var key = Convert.FromBase64String(keyString);

        using (var aes = Aes.Create())
        {
            aes.Key = key;
            
            // Extract IV
            var iv = new byte[IvSize];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            aes.IV = iv;

            // Extract Cipher
            using (var ms = new MemoryStream(fullCipher, IvSize, fullCipher.Length - IvSize))
            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}

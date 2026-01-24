using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class Program
{
    private const string SecretSalt = "AdRev_Security_2026_Robust_Research_System_V2";

    public static void Main()
    {
        string cipherText = "7xbEgxKj/8PWLafKsm72PJ9IsnK8F3L13H3z46TiWm1DR2ssTOnnirrUD2sKsziXkHuZBXOjHz1gIN2lM+mdMWRMF7Vx9Kpq8R6J78EimedTQFAvGO3PTOxStNIKMHQvtT+M1IsTXffrkaMH9GTX2SnAh/Q5tW6WXBmDqNTp7tsxzKKCr+31ZDPu4O151srY0HdUa3DZSSLrXVtq/TcYZwk9HnWkH/EmY3H9hDqnj7s=";
        
        try {
            string plainText = DecryptString(cipherText);
            Console.WriteLine("Decrypted Content:");
            Console.WriteLine(plainText);
        } catch (Exception ex) {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    private static string DecryptString(string cipherText)
    {
        byte[] iv = new byte[16];
        byte[] buffer = Convert.FromBase64String(cipherText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(SecretSalt.Substring(0, 32));
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader streamReader = new StreamReader(cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }
}

using System;
<<<<<<< HEAD
using System.Collections.Generic;
using AdRev.Core.Services;
using AdRev.Domain.Variables;
using AdRev.Domain.Enums;

namespace AdRev.Verify
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ADREV LOGIC VERIFICATION ===");
            
            var service = new RecodeService();
            
            // 1. Test Quantitative Recoding (Range)
            var ages = new List<object> { 12, 18, 25, 45, 10, null, "invalid" };
            var ageInstructions = new List<RecodeInstruction>
            {
                new RecodeInstruction { RangeMin = 0, RangeMax = 18, TargetValue = "Enfant/Ado", IsRange = true },
                new RecodeInstruction { RangeMin = 18, RangeMax = 120, TargetValue = "Adulte", IsRange = true }
            };
            
            var recodedAges = service.Recode(ages, ageInstructions, VariableType.QuantitativeDiscrete);
            
            Console.WriteLine("\n[1] Recodage des Ages (0-18: Enfant/Ado, 18+: Adulte):");
            for(int i=0; i<ages.Count; i++)
            {
                Console.WriteLine($"{ages[i] ?? "NULL"} -> {recodedAges[i]}");
            }

            // 2. Test Qualitative Recoding (Exact Match)
            var gender = new List<object> { "M", "F", "m", "f", "Inconnu" };
            var genderInstructions = new List<RecodeInstruction>
            {
                new RecodeInstruction { SourceValue = "M", TargetValue = "Homme" },
                new RecodeInstruction { SourceValue = "F", TargetValue = "Femme" }
            };
            
            var recodedGender = service.Recode(gender, genderInstructions, VariableType.Text);
            
            Console.WriteLine("\n[2] Recodage du Sexe (M->Homme, F->Femme):");
            for(int i=0; i<gender.Count; i++)
            {
                Console.WriteLine($"{gender[i]} -> {recodedGender[i]}");
            }

            Console.WriteLine("\n=== VERIFICATION COMPLETE ===");
=======
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
>>>>>>> origin/main
        }
    }
}

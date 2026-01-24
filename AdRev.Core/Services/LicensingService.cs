using System;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;
using AdRev.Domain.Models;

namespace AdRev.Core.Services
{
    public class LicensingService
    {
        private const string RegistryPath = @"Software\AdRev";
        private const string LicenseValueName = "LicenseData";
        private const string SecretSalt = "AdRev_Security_2026_Robust_Research_System_V2";

        public string GetHardwareId()
        {
            try
            {
                string hwid = string.Empty;
                using (var cpuSearcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (var obj in cpuSearcher.Get())
                    {
                        hwid += obj["ProcessorId"]?.ToString() ?? "";
                        break;
                    }
                }
                using (var baseSearcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                {
                    foreach (var obj in baseSearcher.Get())
                    {
                        hwid += obj["SerialNumber"]?.ToString() ?? "";
                        break;
                    }
                }
                return HashString(hwid).Substring(0, 16).ToUpper();
            }
            catch
            {
                return HashString(Environment.MachineName).Substring(0, 16).ToUpper();
            }
        }

        public bool IsActivated(out string message)
        {
            message = string.Empty;
            string storedData = GetStoredLicenseData();
            if (string.IsNullOrEmpty(storedData))
            {
                message = "Aucune licence trouvée.";
                return false;
            }

            try
            {
                var metadata = DecryptAndValidate(storedData);
                if (metadata == null)
                {
                    message = "Licence corrompue ou invalide.";
                    return false;
                }

                // 1. Check HWID (Skip if Institutional/Enterprise for flexibility)
                if (metadata.Type != LicenseType.Enterprise && metadata.Hwid != GetHardwareId())
                {
                    message = "Cette licence n'est pas valide pour cet ordinateur.";
                    return false;
                }

                // 2. Check Expiry
                if (metadata.ExpiryDate < DateTime.UtcNow)
                {
                    if (metadata.Type == LicenseType.Trial)
                        message = $"Votre période d'essai a expiré le {metadata.ExpiryDate.ToShortDateString()}.";
                    else
                        message = $"Votre licence annuelle a expiré le {metadata.ExpiryDate.ToShortDateString()}.";
                    return false;
                }

                string label = !string.IsNullOrEmpty(metadata.FeaturesLabel) ? metadata.FeaturesLabel :
                               metadata.Type == LicenseType.Lifetime ? "Licence Professionnelle à Vie" : 
                               metadata.Type == LicenseType.Annual ? "Licence Annuelle" :
                               metadata.Type == LicenseType.Student ? "Licence Étudiant" :
                               metadata.Type == LicenseType.Enterprise ? "Licence Entreprise" :
                               "Essai Gratuit";
                               
                if (metadata.Type != LicenseType.Lifetime && metadata.Type != LicenseType.Enterprise)
                    label += $" (Expire le {metadata.ExpiryDate.ToShortDateString()})";

                if (!string.IsNullOrEmpty(metadata.RegisteredEmail))
                    label += $"\nEnregistré pour : {metadata.RegisteredEmail}";

                message = label;
                return true;
            }
            catch
            {
                message = "Erreur de validation de licence.";
                return false;
            }
        }

        public void StartTrial()
        {
            var metadata = new LicenseMetadata
            {
                Hwid = GetHardwareId(),
                Type = LicenseType.Trial,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                MaxSeats = 1
            };
            
            // Generate Signature (Updated v2 format)
            string dataToSign = $"{metadata.Hwid}|{metadata.Type}|{metadata.ExpiryDate:yyyy-MM-dd}|{metadata.MaxSeats}|{metadata.RegisteredEmail ?? ""}|{metadata.FeaturesLabel ?? ""}";
            metadata.Signature = HashString(dataToSign + SecretSalt);

            // Serialize & Encrypt
            string json = JsonSerializer.Serialize(metadata);
            string encrypted = EncryptString(json);
            
            SaveLicenseData(encrypted);
        }

        public LicenseMetadata? GetCurrentLicense()
        {
            string storedData = GetStoredLicenseData();
            if (string.IsNullOrEmpty(storedData)) return null;
            return DecryptAndValidate(storedData);
        }

        public bool Activate(string licenseKey)
        {
            var metadata = DecryptAndValidate(licenseKey);
            if (metadata != null)
            {
                // Verify if it belongs to this PC (unless it's an Enterprise key)
                if (metadata.Type != LicenseType.Enterprise && metadata.Hwid != GetHardwareId())
                    return false;

                SaveLicenseData(licenseKey);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Génère une nouvelle licence cryptée (Pour usage CLI / Bot).
        /// </summary>
        public string GenerateLicense(string hwid, LicenseType type, int maxSeats, DateTime expiryDate, string email, string label)
        {
            var metadata = new LicenseMetadata
            {
                Hwid = hwid,
                Type = type,
                ExpiryDate = expiryDate,
                MaxSeats = maxSeats,
                RegisteredEmail = email,
                FeaturesLabel = label
            };

            // Generate Signature
            string dataToSign = $"{metadata.Hwid}|{metadata.Type}|{metadata.ExpiryDate:yyyy-MM-dd}|{metadata.MaxSeats}|{metadata.RegisteredEmail ?? ""}|{metadata.FeaturesLabel ?? ""}";
            metadata.Signature = HashString(dataToSign + SecretSalt);

            // Serialize & Encrypt
            string json = JsonSerializer.Serialize(metadata);
            return EncryptString(json);
        }

        /// <summary>
        /// Décrypte une licence pour inspection (Support).
        /// </summary>
        public LicenseMetadata? InspectLicense(string licenseKey)
        {
            return DecryptAndValidate(licenseKey);
        }

        private LicenseMetadata? DecryptAndValidate(string encryptedKey)
        {
            try
            {
                string json = DecryptString(encryptedKey);
                var metadata = JsonSerializer.Deserialize<LicenseMetadata>(json);
                if (metadata == null) return null;

                // Verify Signature (Updated v2 format)
                string dataToSign = $"{metadata.Hwid}|{metadata.Type}|{metadata.ExpiryDate:yyyy-MM-dd}|{metadata.MaxSeats}|{metadata.RegisteredEmail ?? ""}|{metadata.FeaturesLabel ?? ""}";
                string expectedSignature = HashString(dataToSign + SecretSalt);

                if (metadata.Signature == expectedSignature)
                    return metadata;
            }
            catch { }
            return null;
        }

        private string HashString(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "");
            }
        }

        private void SaveLicenseData(string data)
        {
            using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                rk.SetValue(LicenseValueName, data);
            }
        }

        private string GetStoredLicenseData()
        {
            using (RegistryKey? rk = Registry.CurrentUser.OpenSubKey(RegistryPath))
            {
                return rk?.GetValue(LicenseValueName)?.ToString() ?? string.Empty;
            }
        }

        // Simple AES Encryption for the Payload
        private string EncryptString(string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(SecretSalt.Substring(0, 32));
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(array);
        }

        private string DecryptString(string cipherText)
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
}

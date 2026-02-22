using System;
using System.IO;
using System.Text.Json;

namespace AdRev.Core.Services
{
    public class ResearcherProfile
    {
        public string FullName { get; set; } = "Nouveau Profil";
        public string Title { get; set; } = "Chercheur Principal";
        public string Institution { get; set; } = string.Empty;
    }

    public class ResearcherProfileService
    {
        private readonly string _filePath;

        public ResearcherProfileService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _filePath = Path.Combine(appData, "AdRev", "researcher_profile.json");
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        }

        public ResearcherProfile GetProfile()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    byte[] encryptedData = File.ReadAllBytes(_filePath);
                    byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(encryptedData, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                    string json = System.Text.Encoding.UTF8.GetString(decryptedData);
                    return JsonSerializer.Deserialize<ResearcherProfile>(json) ?? new ResearcherProfile();
                }
            }
            catch { }
            return new ResearcherProfile();
        }

        public void SaveProfile(ResearcherProfile profile)
        {
            try
            {
                string json = JsonSerializer.Serialize(profile);
                byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
                byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(data, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                File.WriteAllBytes(_filePath, encryptedData);
            }
            catch { }
        }
    }
}

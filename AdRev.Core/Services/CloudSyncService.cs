using System;
using System.IO;
using System.Collections.Generic;

namespace AdRev.Core.Services
{
    public class CloudSyncService
    {
        public bool IsPathInCloud(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            
            string lowerPath = path.ToLower();
            return lowerPath.Contains("onedrive") || 
                   lowerPath.Contains("google drive") || 
                   lowerPath.Contains("dropbox") || 
                   lowerPath.Contains("icloud");
        }

        public List<CloudProviderInfo> GetAvailableCloudProviders()
        {
            var providers = new List<CloudProviderInfo>();
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // OneDrive
            string oneDrive = Path.Combine(userProfile, "OneDrive");
            if (Directory.Exists(oneDrive)) 
                providers.Add(new CloudProviderInfo { Name = "OneDrive", Path = oneDrive, Type = CloudType.OneDrive });

            // Google Drive
            string googleDrive = Path.Combine(userProfile, "Google Drive");
            if (Directory.Exists(googleDrive))
                providers.Add(new CloudProviderInfo { Name = "Google Drive", Path = googleDrive, Type = CloudType.GoogleDrive });
                
            return providers;
        }
    }

    public enum CloudType { OneDrive, GoogleDrive, Dropbox, Unknown }

    public class CloudProviderInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public CloudType Type { get; set; } = CloudType.Unknown;
    }
}

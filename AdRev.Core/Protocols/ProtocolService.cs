using AdRev.Domain.Protocols;
using System.IO;
using System.Text.Json;

namespace AdRev.Core.Protocols
{
    public class ProtocolService
    {
        private readonly string _storagePath;

        public ProtocolService()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AdRev", "Protocols");
            Directory.CreateDirectory(folder);
            _storagePath = folder;
        }

        public ResearchProtocol Create(ResearchProtocol protocol, string? projectName = null)
        {
            // ID is already generated in Model constructor as GUID
            if (string.IsNullOrEmpty(protocol.Id))
            {
                protocol.Id = Guid.NewGuid().ToString();
            }
            
            // Determine Folder Path
            string targetFolder = _storagePath; // Default "Protocols"
            
            if (!string.IsNullOrWhiteSpace(projectName))
            {
                // Sanitize Project Name
                var safeName = string.Join("_", projectName.Split(Path.GetInvalidFileNameChars()));
                // Use a dedicated Projects folder structure ? Or just mix in Protocols ? 
                // Let's make a dedicated Project Folder: AdRev/Projects/[ProjectName]
                
                var projectsRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AdRev", "Projects");
                var projectDir = Path.Combine(projectsRoot, safeName); // Uniqueness?
                
                // Ensure unique if new, or use existing if updating? 
                // For simplified flow, assume 1 project = 1 folder.
                Directory.CreateDirectory(projectDir);
                
                // Create References subfolder
                Directory.CreateDirectory(Path.Combine(projectDir, "References"));
                
                targetFolder = projectDir;
            }

            var fileName = $"Protocol_{protocol.Id}.json"; // Fixed name so we overwrite checks? OR Timestamped?
            // User requested "keep documents... access at any time". Overwriting 'latest' seems better for a "Save" button than creating history files. Or maybe maintain history.
            // Let's stick to unique file for now, or maybe simplified "protocol.json" if inside a project folder?
            // Let's keep ID to be safe from conflict.
            
            var filePath = Path.Combine(targetFolder, fileName);

            var json = JsonSerializer.Serialize(protocol, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);

            return protocol;
        }

        public List<ResearchProtocol> GetAllProtocols()
        {
            var protocols = new List<ResearchProtocol>();
            if (!Directory.Exists(_storagePath)) return protocols;

            // Search in default folder
            // Use search pattern for json files that look like protocols
            var files = Directory.GetFiles(_storagePath, "Protocol_*.json", SearchOption.AllDirectories);
            
            foreach (var file in files) 
            {
                try 
                {
                    var json = File.ReadAllText(file);
                    // Simple check or try deserialize
                    var p = JsonSerializer.Deserialize<ResearchProtocol>(json);
                    if (p != null) protocols.Add(p);
                }
                catch { }
            }
            // Sort by Date Descending
            protocols.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt));
            return protocols;
        }
    }
}

using AdRev.Domain.Enums;
using AdRev.Domain.Models;
using System.IO;
using System.Text.Json;

namespace AdRev.Core.Common
{
    public class ResearchProjectService
    {
        private readonly string _storagePath;

        public ResearchProjectService()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AdRev", "Projects");
            Directory.CreateDirectory(folder);
            _storagePath = folder;
        }

        public ResearchProject CreateNew(string title, StudyType studyType, ProjectContext context, ScientificDomain domain, string authors, string institution, string? customPath = null)
        {
            var project = new ResearchProject
            {
                Title = title,
                StudyType = studyType,
                Context = context,
                Domain = domain,
                Authors = authors,
                Institution = institution,
                CreatedOn = DateTime.Now,
                Version = "1.0.0"
            };

            // Generate clean folder name based on Title + Date
            string safeTitle = string.Join("_", title.Split(Path.GetInvalidFileNameChars()));
            string folderName = $"{safeTitle}_{DateTime.Now:yyyyMMdd}";
            
            string baseDir = _storagePath;
            if (!string.IsNullOrWhiteSpace(customPath) && Directory.Exists(customPath))
            {
                baseDir = customPath;
            }

            string projectDir = Path.Combine(baseDir, folderName);
            if (!Directory.Exists(projectDir)) Directory.CreateDirectory(projectDir);

            // Create Scientific Folder Hierarchy
            Directory.CreateDirectory(Path.Combine(projectDir, "01_Protocol"));
            Directory.CreateDirectory(Path.Combine(projectDir, "02_Data", "Raw"));
            Directory.CreateDirectory(Path.Combine(projectDir, "02_Data", "Processed"));
            Directory.CreateDirectory(Path.Combine(projectDir, "03_Analysis"));
            Directory.CreateDirectory(Path.Combine(projectDir, "04_Manuscripts"));
            Directory.CreateDirectory(Path.Combine(projectDir, "99_Resources"));

            // Save Project File
            string fileName = "adrev_project.json";
            string filePath = Path.Combine(projectDir, fileName);
            project.FilePath = filePath;

            var json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);

            // Create Link if needed (if created outside default storage)
            if (baseDir != _storagePath)
            {
                var linkContent = new { LinkedPath = filePath };
                string linkFile = Path.Combine(_storagePath, $"Link_{DateTime.Now:yyyyMMddHHmmss}.json");
                File.WriteAllText(linkFile, JsonSerializer.Serialize(linkContent));
            }

            return project;
        }
        public ResearchProject CreateProjectFromProtocol(AdRev.Domain.Protocols.ResearchProtocol protocol)
        {
            if (protocol == null) throw new ArgumentNullException(nameof(protocol));

            var project = CreateNew(
                title: protocol.Title,
                studyType: protocol.StudyType,
                context: ProjectContext.Other, // Default or map if possible
                domain: protocol.Domain,
                authors: $"{protocol.PrincipalAuthor?.FirstName} {protocol.PrincipalAuthor?.LastName}",
                institution: protocol.PrincipalAuthor?.Institution ?? "Unknown"
            );

            // Populate Additional Data
            project.SourceProtocolId = protocol.Id;
            project.GeneralObjective = protocol.GeneralObjective;
            project.ProblemJustification = protocol.ProblemJustification ?? protocol.Context;
            project.DataAnalysisPlan = protocol.DataAnalysis;
            
            // Map other useful fields if they exist in Project model
            project.LimitationsContent = protocol.StudyLimitations;
            
            // Save updates
            SaveProject(project);

            return project;
        }

        public ResearchProject ImportFromFolder(string folderPath)
        {
            // 1. Check for standard "adrev_project.json"
            string standardFile = Path.Combine(folderPath, "adrev_project.json");
            if (File.Exists(standardFile))
            {
                 // Create Link
                var linkContent = new { LinkedPath = standardFile };
                string linkFile = Path.Combine(_storagePath, $"Link_Import_{Guid.NewGuid()}.json");
                File.WriteAllText(linkFile, JsonSerializer.Serialize(linkContent));
                
                var json = File.ReadAllText(standardFile);
                return JsonSerializer.Deserialize<ResearchProject>(json) ?? new ResearchProject();
            }

            // 2. Fallback: Check for legacy "Project_*.json"
            var existingFiles = Directory.GetFiles(folderPath, "Project_*.json");
            if (existingFiles.Any())
            {
                var linkContent = new { LinkedPath = existingFiles.First() };
                string linkFile = Path.Combine(_storagePath, $"Link_Import_{Guid.NewGuid()}.json");
                File.WriteAllText(linkFile, JsonSerializer.Serialize(linkContent));
                var json = File.ReadAllText(existingFiles.First());
                var proj = JsonSerializer.Deserialize<ResearchProject>(json);
                if (proj != null) proj.FilePath = existingFiles.First(); // Ensure path set
                return proj ?? new ResearchProject();
            }
            else
            {
                // 3. Import RAW folder -> Create Wrapper + Structure
                string dirName = new DirectoryInfo(folderPath).Name;
                // We create a NEW project structure INSIDE the selected folder to avoid moving files? 
                // OR we treat the folder AS the project root and add our structure.
                // Let's add our structure non-destructively.
                
                // Create Scientific Folder Hierarchy (if missing)
                Directory.CreateDirectory(Path.Combine(folderPath, "01_Protocol"));
                Directory.CreateDirectory(Path.Combine(folderPath, "02_Data", "Raw"));
                Directory.CreateDirectory(Path.Combine(folderPath, "02_Data", "Processed"));
                Directory.CreateDirectory(Path.Combine(folderPath, "03_Analysis"));
                Directory.CreateDirectory(Path.Combine(folderPath, "04_Manuscripts"));
                Directory.CreateDirectory(Path.Combine(folderPath, "99_Resources"));

                // Create Project File
                var project = new ResearchProject
                {
                    Title = dirName,
                    StudyType = StudyType.Mixed,
                    Context = ProjectContext.Other,
                    Domain = ScientificDomain.Biomedical,
                    Authors = "Imported User",
                    Institution = "Local",
                    CreatedOn = DateTime.Now,
                    Version = "1.0.0",
                    FilePath = Path.Combine(folderPath, "adrev_project.json")
                };

                var json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(project.FilePath, json);

                // Link it
                var linkContent = new { LinkedPath = project.FilePath };
                string linkFile = Path.Combine(_storagePath, $"Link_Import_{Guid.NewGuid()}.json");
                File.WriteAllText(linkFile, JsonSerializer.Serialize(linkContent));

                return project;
            }
        }

        public List<ResearchProject> GetAllProjects()
        {
            var projects = new List<ResearchProject>();
            if (!Directory.Exists(_storagePath)) return projects;

<<<<<<< HEAD
            // 1. Load Local Projects (Legacy + New) - RECURSIVE SEARCH
            var files = Directory.GetFiles(_storagePath, "Project_*.json", SearchOption.AllDirectories).ToList();
            files.AddRange(Directory.GetFiles(_storagePath, "adrev_project.json", SearchOption.AllDirectories));
=======
            // 1. Load Local Projects (Legacy + New)
            var files = Directory.GetFiles(_storagePath, "Project_*.json").ToList();
            files.AddRange(Directory.GetFiles(_storagePath, "adrev_project.json")); // unlikely in root but possible
>>>>>>> origin/main

            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var project = JsonSerializer.Deserialize<ResearchProject>(json);
                    if (project != null)
                    {
                        project.FilePath = file;
                        projects.Add(project);
                    }
                }
                catch { /* Skip */ }
            }

            // 2. Load Linked Projects
            var links = Directory.GetFiles(_storagePath, "Link_*.json");
            foreach (var link in links)
            {
                try
                {
                    var jsonLink = File.ReadAllText(link);
                    using (JsonDocument doc = JsonDocument.Parse(jsonLink))
                    {
                        if (doc.RootElement.TryGetProperty("LinkedPath", out JsonElement pathEl))
                        {
                            string? targetPath = pathEl.GetString();
                            if (!string.IsNullOrEmpty(targetPath) && File.Exists(targetPath))
                            {
                                var jsonProj = File.ReadAllText(targetPath);
                                var project = JsonSerializer.Deserialize<ResearchProject>(jsonProj);
                                if (project != null)
                                {
                                    project.FilePath = targetPath;
                                    // Avoid duplicates if somehow linked and local overlap
                                    if (!projects.Any(p => p.FilePath == targetPath))
                                        projects.Add(project);
                                }
                            }
                        }
                    }
                }
                catch { /* Skip broken links */ }
            }

            return projects.OrderByDescending(p => p.CreatedOn).ToList();
        }

        public void SaveProject(ResearchProject project)
        {
            if (project != null && !string.IsNullOrEmpty(project.FilePath))
            {
                var json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(project.FilePath, json);
            }
        }

        public void DeleteProject(ResearchProject project)
        {
            if (project != null && !string.IsNullOrEmpty(project.FilePath) && File.Exists(project.FilePath))
            {
                File.Delete(project.FilePath);
            }
        }
    }
}


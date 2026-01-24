using System;
using AdRev.Domain.Enums;
using AdRev.Domain.Variables;
using System.Collections.Generic;
using System.ComponentModel;

namespace AdRev.Domain.Models
{
    public enum ProjectStatus
    {
        [Description("En Cours")]
        Ongoing,
        [Description("Terminé")]
        Completed,
        [Description("Accepté / Validé")]
        Accepted,
        [Description("Publié")]
        Published,
        [Description("Archivé")]
        Archived
    }

    public class ResearchProject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0.0"; // Semantic Versioning


        public StudyType StudyType { get; set; }
        public ScientificDomain Domain { get; set; } = ScientificDomain.Biomedical;
        public EpidemiologicalStudyType EpidemiologyType { get; set; }
        public ProjectContext Context { get; set; }
        public AcademicLevel AcademicLevel { get; set; } = AcademicLevel.Thesis;
        public string TargetJournalName { get; set; } = string.Empty;
        // Serialized JSON of journal criteria if needed, or just the name to lookup
        public string TargetJournalCriteriaJson { get; set; } = string.Empty; 

        public string Authors { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;

        public string? FilePath { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.Ongoing;

        public DateTime? PublicationDate { get; set; }
        public string PublicationReference { get; set; } = string.Empty; // Volume, Issue, DOI
        public int AuthorRank { get; set; } = 1; // 1 = First Author

        // Link to Protocol
        public string? SourceProtocolId { get; set; } 

        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public List<StudyVariable> Variables { get; set; } = new List<StudyVariable>();

        // Protocol Data linked to Project
        public string ProblemJustification { get; set; } = string.Empty;
        public string GeneralObjective { get; set; } = string.Empty;
        public string SpecificObjectives { get; set; } = string.Empty; // text blob for now
        
        // This holds the textual SAP from the protocol
        public string DataAnalysisPlan { get; set; } = string.Empty;

        // Results & Discussion (Persisted)
        public string DiscussionContent { get; set; } = string.Empty;
        public string LimitationsContent { get; set; } = string.Empty;
        public string ConclusionContent { get; set; } = string.Empty;
        public List<Citation> References { get; set; } = new List<Citation>();

        public List<Author> Team { get; set; } = new List<Author>();

        // Qualitative Data
        public List<QualitativeSource> QualitativeSources { get; set; } = new List<QualitativeSource>();
        public List<QualitativeCode> QualitativeCodes { get; set; } = new List<QualitativeCode>();
    }
}

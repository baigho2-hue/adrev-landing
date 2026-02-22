using System.Collections.Generic;

namespace AdRev.Domain.Quality
{
    public class QualityChecklist
    {
        public string Name { get; set; } = string.Empty; // e.g., "COREQ", "STROBE"
        public string Description { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public List<ChecklistSection> Sections { get; set; } = new List<ChecklistSection>();
    }

    public class ChecklistSection
    {
        public string Name { get; set; } = string.Empty; // e.g., "Domain 1: Research Team"
        public List<ChecklistItem> Items { get; set; } = new List<ChecklistItem>();
    }

    public class ChecklistItem
    {
        public string Id { get; set; } = string.Empty;
        public string Requirement { get; set; } = string.Empty; // The question/requirement
        public string Description { get; set; } = string.Empty; // More details
        public bool IsMet { get; set; } = false;
        public string UserNotes { get; set; } = string.Empty;
    }
}

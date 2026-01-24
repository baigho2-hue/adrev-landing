using System;
using System.Collections.Generic;

namespace AdRev.Domain.Models
{
    public class JournalSubmissionCriteria
    {
        public string JournalName { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public List<string> Specialties { get; set; } = new List<string>(); // e.g. "Primary Care", "General Medicine"
        
        // General Constraints
        public int MaxWordCountAbstract { get; set; }
        public int MaxWordCountBody { get; set; }
        public int MaxFiguresAndTables { get; set; }
        public int MaxReferences { get; set; }
        
        // Structure Requirements
        public bool RequiresStructuredAbstract { get; set; } // e.g., Background, Methods, Results, Conclusion
        public List<string> RequiredSections { get; set; } = new List<string>();
        
        // Formatting Style
        public string CitationStyle { get; set; } = string.Empty; // e.g., "Vancouver", "APA"
        public string FontRequirements { get; set; } = string.Empty;
        
        // Submission Info
        public string SubmissionUrl { get; set; } = string.Empty;
        public string GuidelinesUrl { get; set; } = string.Empty;

        public JournalSubmissionCriteria() { }
    }
}

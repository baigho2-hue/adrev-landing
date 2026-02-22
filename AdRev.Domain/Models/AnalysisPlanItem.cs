using System;
using System.Collections.Generic;

namespace AdRev.Domain.Models
{
    public class AnalysisPlanItem
    {
        public string Title { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty; // e.g., "Descriptive", "T-Test", "ANOVA", "Regression"
        public string Description { get; set; } = string.Empty; // Auto-generated or manual text
        
        // Selected variable names
        public List<string> Variables { get; set; } = new List<string>();

        // Explicit bindings for UI
        public string Variable1 { get; set; } = string.Empty; // Primary / Dependent
        public string Variable2 { get; set; } = string.Empty; // Secondary / Independent

        // Options
        public bool IncludeTable { get; set; } = true;
        public bool IncludeChart { get; set; } = true;

        // Status
        public bool IsExecuted { get; set; }
        public string ResultSummary { get; set; } = string.Empty;

        public AnalysisPlanItem() { }
    }
}

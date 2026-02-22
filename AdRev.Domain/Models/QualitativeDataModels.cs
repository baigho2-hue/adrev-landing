using System;
using System.Collections.Generic;

namespace AdRev.Domain.Models
{
    public class QualitativeSource
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Type { get; set; } = "Document"; // "Transcription", "Audio", "PDF"
        public string Content { get; set; } = string.Empty; // For text content
        public DateTime ImportDate { get; set; } = DateTime.Now;
        
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class QualitativeCode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ParentId { get; set; } = string.Empty; // For hierarchy
        public string Color { get; set; } = "#CCCCCC";
        public List<QualitativeCodeSegment> Segments { get; set; } = new List<QualitativeCodeSegment>();
    }

    public class QualitativeCodeSegment
    {
        public string SourceId { get; set; } = string.Empty;
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public string SelectedText { get; set; } = string.Empty;
    }
}

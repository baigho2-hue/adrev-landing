using AdRev.Domain.Enums;
using System;

namespace AdRev.Domain.Models
{
    public class ProtocolAppendix
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public AppendixType Type { get; set; } = AppendixType.Other;
        
        // Detailed content or description of the appendix
        public string Content { get; set; } = string.Empty;
        
        // Optional path if it's an external file
        public string? FilePath { get; set; }

        public override string ToString()
        {
            return $"{Title} ({Type})";
        }
    }
}

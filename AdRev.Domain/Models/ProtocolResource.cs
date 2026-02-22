using System;

namespace AdRev.Domain.Models
{
    public enum ResourceType
    {
        Table,
        Figure,
        Image,
        Other
    }

    public class ProtocolResource
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public ResourceType Type { get; set; } = ResourceType.Table;
        public string FilePath { get; set; } = string.Empty; // Optional: for imported images/files
        public string Description { get; set; } = string.Empty;
        public int Number { get; set; } // The assigned number (e.g., 1 for Figure 1)

        // For "Create" option (simple internal data representation)
        // could be JSON or CSV string for simple tables
        public string ContentData { get; set; } = string.Empty; 
    }
}

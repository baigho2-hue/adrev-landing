using System;

namespace AdRev.Domain.Models
{
    public class AuditLogEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // e.g., "Create", "Update", "Delete"
        public string EntityType { get; set; } = string.Empty; // e.g., "DataRow", "Variable", "LibraryItem"
        public string EntityId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty; // JSON or text description of the change
    }
}

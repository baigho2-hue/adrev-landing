using System;

namespace AdRev.Domain.Models
{
    public class PurchaseRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Hwid { get; set; } = string.Empty;
        public LicenseType RequestedType { get; set; } = LicenseType.Pro;
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public bool IsProcessed { get; set; } = false;
        public string GeneratedLicenseKey { get; set; } = string.Empty;
        
        // Metadata
        public string Location { get; set; } = string.Empty; // Pays/Ville
        public string TransactionId { get; set; } = string.Empty;
    }
}

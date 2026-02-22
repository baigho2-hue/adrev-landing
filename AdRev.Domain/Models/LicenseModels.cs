using System;

namespace AdRev.Domain.Models
{
    public enum LicenseType
    {
        Student,
        Pro,
        Elite,
        Enterprise,
        Unlimited,
        Trial
    }

    public class LicenseMetadata
    {
        public string Hwid { get; set; } = string.Empty;
        public LicenseType Type { get; set; } = LicenseType.Unlimited;
        public DateTime ExpiryDate { get; set; } = DateTime.MaxValue;
        public int MaxSeats { get; set; } = 1;
        
        /// <summary>
        /// Email du client enregistré (Affichage Pro).
        /// </summary>
        public string RegisteredEmail { get; set; } = string.Empty;

        /// <summary>
        /// Label commercial (ex: "Licence Étudiant - Mali").
        /// </summary>
        public string FeaturesLabel { get; set; } = string.Empty;

        public string Signature { get; set; } = string.Empty;
    }
}

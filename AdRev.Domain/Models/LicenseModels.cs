using System;

namespace AdRev.Domain.Models
{
    public enum LicenseType
    {
<<<<<<< HEAD
        Student,
        Pro,
        Elite,
        Enterprise,
        Unlimited,
=======
        Lifetime,
        Annual,
        Enterprise,
        Student,
>>>>>>> origin/main
        Trial
    }

    public class LicenseMetadata
    {
        public string Hwid { get; set; } = string.Empty;
<<<<<<< HEAD
        public LicenseType Type { get; set; } = LicenseType.Unlimited;
=======
        public LicenseType Type { get; set; } = LicenseType.Lifetime;
>>>>>>> origin/main
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

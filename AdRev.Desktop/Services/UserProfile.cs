
using System;

namespace AdRev.Desktop.Services
{
    public class UserProfile
    {
        public string Title { get; set; } = ""; // M. / Mme / Dr / Pr
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        
        public bool IsSet => !string.IsNullOrWhiteSpace(LastName);
    }
}

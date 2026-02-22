using System;
using System.Security.Cryptography;
using System.Text;

namespace AdRev.Core.Services
{
    public static class SecurityService
    {
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public static bool VerifyPassword(string password, string? hash)
        {
            if (string.IsNullOrEmpty(hash)) return true; // Not protected
            string newHash = HashPassword(password);
            return newHash == hash;
        }
    }
}

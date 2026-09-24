using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AWSGateway.Helpers
{
    public static class CryptoHelper
    {
        public static string GenerateSalt(string username, DateTime registrationDate)
        {
            string saltInput = username + ":" + registrationDate.ToString("yyyy-MM-dd");

            using (SHA256 sha = SHA256.Create())
            {
                byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(saltInput));
                return Convert.ToBase64String(hashBytes);
            }
        }

        public static string HashPassword(string password, string salt)
        {
            string combined = salt + password;

            using (SHA256 sha = SHA256.Create())
            {
                byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool VerifyPassword(string inputPassword, string storedHash, string salt)
        {
            string hash = HashPassword(inputPassword, salt);
            return hash == storedHash;
        }
        
    }
}
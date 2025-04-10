using Imel.Database;
using Imel.Database.Models;
using Imel.Models.User;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Imel.Services
{
    public static class Helpers
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            using (Rfc2898DeriveBytes bytes = new(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }

        public static bool VerifyPassword(string password, string passwordHash)
        {
            byte[] hashBytes = Convert.FromBase64String(passwordHash);
            if (hashBytes.Length != 49 || hashBytes[0] != 0) return false;
            byte[] salt = new byte[16];
            Buffer.BlockCopy(hashBytes, 1, salt, 0, 16);
            byte[] storedSubkey = new byte[32];
            Buffer.BlockCopy(hashBytes, 17, storedSubkey, 0, 32);
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 1000))
            {
                byte[] generatedSubkey = deriveBytes.GetBytes(32);
                return storedSubkey.SequenceEqual(generatedSubkey);
            }
        }

        public static void CreateUserVersion(User user, string action)
        {
            var version = new UserVersion
            {
                Id = DBContext.UserVersions.Any() ? DBContext.UserVersions.Max(x => x.Id) + 1 : 1,
                UserId = user.Id,
                UserData = new User()
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    PasswordHash = user.PasswordHash,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastModified = user.LastModified,
                    RoleId = user.RoleId,
                    Role = user.Role
                },
                VersionNumber = DBContext.UserVersions.Any() ? DBContext.UserVersions.Max(x => x.VersionNumber) + 1 : 1,
                Action = action
            };
            DBContext.UserVersions.Add(version);
        }
    }
}
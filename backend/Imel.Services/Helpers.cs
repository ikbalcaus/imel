using Imel.Database;
using Imel.Database.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Imel.Services
{
    public static class Helpers
    {
        public static IHttpContextAccessor? HttpContextAccessor { get; set; }

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
                UserData = CloneUser(user),
                VersionNumber = (DBContext.UserVersions.LastOrDefault(x => x.UserId == user.Id)?.VersionNumber ?? 0) + 1,
                Action = action,
                ModifiedByUser = GetUserFromToken().Username
            };
            DBContext.UserVersions.Add(version);
        }

        public static void CreateAuditLog(string entityType, int entityId, string action, object oldValues, object newValues)
        {
            var log = new AuditLog
            {
                Id = DBContext.AuditLogs.Count + 1,
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                OldValues = JsonSerializer.Serialize(oldValues),
                NewValues = JsonSerializer.Serialize(newValues),
                Timestamp = DateTime.UtcNow,
                ModifiedByUser = GetUserFromToken().Username
            };
            DBContext.AuditLogs.Add(log);
        }

        public static User CloneUser(User user)
        {
            var role = DBContext.Roles.FirstOrDefault(x => x.Id == user.RoleId);
            var clonedUser = new User()
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                PasswordHash = user.PasswordHash,
                IsActive = user.IsActive,
                RoleId = user.RoleId,
                Role = new Role
                {
                    Id = role!.Id,
                    Name = role!.Name
                }
            };
            return clonedUser;
        }

        public static User GetUserFromToken()
        {
            var token = HttpContextAccessor?.HttpContext?.User;

            if (token == null)
            {
                return new User();
            }

            var email = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return new User();
            }

            var user = DBContext.Users.FirstOrDefault(u => u.Email == email);
            return user ?? new User();
        }
    }
}
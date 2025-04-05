using Imel.Database;
using Imel.Interfaces;
using Imel.Models.Auth;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Imel.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private const int MaxLoginAttempts = 5;
        private readonly TimeSpan LoginAttemptWindow = TimeSpan.FromMinutes(15);
        private readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(30);

        private static readonly Dictionary<string, User> _users = new();

        public AuthService(IConfiguration configuration, IMemoryCache cache)
        {
            _configuration = configuration;
            _cache = cache;
        }

        public string HashPassword(string password)
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

        private bool VerifyPassword(string hashedPassword, string password)
        {
            byte[] buffer4;
            byte[] src = Convert.FromBase64String(hashedPassword);

            if (src.Length != 0x31 || src[0] != 0)
            {
                return false;
            }

            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            byte[] buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);

            using (Rfc2898DeriveBytes bytes = new(password, dst, 0x3e8))
            {
                buffer4 = bytes.GetBytes(0x20);
            }

            return buffer3.SequenceEqual(buffer4);
        }

        public int GetRemainingAttempts(string username)
        {
            var cacheKey = $"login_attempts_{username}";
            return _cache.TryGetValue(cacheKey, out int attempts)
                ? MaxLoginAttempts - attempts
                : MaxLoginAttempts;
        }

        public void AddUser(string email, string username, string password, string? role = null)
        {
            _users[username] = new User
            {
                Email = email,
                Username = username,
                PasswordHash = HashPassword(password),
                Role = role
            };
        }

        public AuthResult ValidateUser(LoginRequest loginRequest)
        {
            var cacheKey = $"login_attempts_{loginRequest.Email}";
            var lockoutKey = $"account_lockout_{loginRequest.Email}";

            if (_cache.TryGetValue(lockoutKey, out DateTime lockoutEnd) && DateTime.UtcNow < lockoutEnd)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = $"Account locked. Please try again after {lockoutEnd.Subtract(DateTime.UtcNow):mm} minutes.",
                    RemainingAttempts = 0
                };
            }

            var attempts = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = LoginAttemptWindow;
                return 0;
            });

            if (attempts >= MaxLoginAttempts)
            {
                var lockoutEndTime = DateTime.UtcNow.Add(LockoutDuration);
                _cache.Set(lockoutKey, lockoutEndTime, LockoutDuration);
                _cache.Remove(cacheKey);

                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = $"Too many attempts. Account is locked until {lockoutEndTime:T}.",
                    RemainingAttempts = 0
                };
            }

            if (!_users.TryGetValue(loginRequest.Email, out var user) ||
                !VerifyPassword(user.PasswordHash, loginRequest.Password))
            {
                _cache.Set(cacheKey, attempts + 1, LoginAttemptWindow);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Invalid credentials",
                    RemainingAttempts = MaxLoginAttempts - (attempts + 1)
                };
            }

            _cache.Remove(cacheKey);
            _cache.Remove(lockoutKey);
            return new AuthResult { Success = true, RemainingAttempts = MaxLoginAttempts };
        }

        public LoginResponse GenerateToken(string email)
        {
            if (!_users.TryGetValue(email, out var user))
                throw new ArgumentException("User does not exist");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username)
            };

            if (!string.IsNullOrEmpty(user.Role))
            {
                claims.Add(new(ClaimTypes.Role, user.Role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new LoginResponse
            {
                Token = tokenHandler.WriteToken(token),
                Expiration = tokenDescriptor.Expires.Value
            };
        }
    }
}
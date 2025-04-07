using Imel.Database.Models;
using Imel.Database;
using Imel.Interfaces;
using Imel.Models.Auth;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Imel.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private const int MaxLoginAttempts = 5;
        private readonly TimeSpan LoginAttemptWindow = TimeSpan.FromMinutes(15);
        private readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(30);

        public AuthService(IConfiguration configuration, IMemoryCache cache)
        {
            _configuration = configuration;
            _cache = cache;
        }

        public int GetRemainingAttempts(string email)
        {
            var cacheKey = $"login_attempts_{email}";
            return _cache.TryGetValue(cacheKey, out int attempts)
                ? MaxLoginAttempts - attempts
                : MaxLoginAttempts;
        }

        public void AddUser(string email, string username, string password, int roleId)
        {
            if (DBContext.Users.ContainsKey(email))
            {
                throw new ArgumentException("User with this email already exists");
            }

            if (DBContext.Users.Values.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Username is already taken");
            }

            if (!Helpers.IsValidEmail(email))
            {
                throw new ArgumentException("Invalid email format");
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                throw new ArgumentException("Password must be at least 8 characters long");
            }

            DBContext.Users[email] = new User
            {
                Email = email,
                Username = username,
                PasswordHash = Helpers.HashPassword(password),
                RoleId = (roleId == 1 || roleId == 2) ? roleId : 1,
                Role = DBContext.Roles.FirstOrDefault(x => (x.Id == roleId || x.Id == 1))!
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

            if (!DBContext.Users.TryGetValue(loginRequest.Email, out var user) ||
                !Helpers.VerifyPassword(user.PasswordHash, loginRequest.Password))
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
            if (!DBContext.Users.TryGetValue(email, out var user))
                throw new ArgumentException("User does not exist");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Email)
            };

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
                Expiration = tokenDescriptor.Expires.Value,
                Message = "User loggedin successfully"
            };
        }
    }
}
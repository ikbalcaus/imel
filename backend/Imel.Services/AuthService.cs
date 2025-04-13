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
            return _cache.TryGetValue(cacheKey, out int attempts) ? MaxLoginAttempts - attempts : MaxLoginAttempts;
        }

        public void AddUser(RegisterRequest req)
        {
            if (DBContext.Users.Any(x => x.Email.Equals(req.Email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Email is already taken");
            }

            if (DBContext.Users.Any(x => x.Username.Equals(req.Username, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Username is already taken");
            }

            if (!Helpers.IsValidEmail(req.Email))
            {
                throw new ArgumentException("Invalid email format");
            }

            if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 8)
            {
                throw new ArgumentException("Password must be at least 8 characters long");
            }

            int id = DBContext.Users.Any() ? DBContext.Users.Max(x => x.Id) + 1 : 1;

            var user = new User
            {
                Id = id,
                Email = req.Email,
                Username = req.Username,
                PasswordHash = Helpers.HashPassword(req.Password),
                RoleId = 1,
                Role = DBContext.Roles.FirstOrDefault(x => (x.Id == 1))!
            };
            DBContext.Users.Add(user);
            Helpers.CreateUserVersion(user, "CREATE");
            Helpers.CreateAuditLog("User", id, "CREATE", "", user);
        }

        public AuthResult ValidateUser(LoginRequest req)
        {
            var cacheKey = $"login_attempts_{req.Email}";
            var lockoutKey = $"account_lockout_{req.Email}";

            if (_cache.TryGetValue(lockoutKey, out DateTime lockoutEnd) && DateTime.UtcNow < lockoutEnd)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = $"Account locked. Please try again after {lockoutEnd.Subtract(DateTime.UtcNow):mm} minutes.",
                    RemainingAttempts = 0
                };
            }

            var user = DBContext.Users.FirstOrDefault(x => x.Email == req.Email);
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

            if (!DBContext.Users.Any(x => x.Email == req.Email) || !Helpers.VerifyPassword(req.Password, user!.PasswordHash))
            {
                _cache.Set(cacheKey, attempts + 1, LoginAttemptWindow);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Invalid credentials",
                    RemainingAttempts = MaxLoginAttempts - (attempts + 1)
                };
            }

            if (DBContext.Users.FirstOrDefault(x => x.Email == req.Email)!.IsDeleted == true)
            {
                _cache.Set(cacheKey, attempts + 1, LoginAttemptWindow);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Your Account is Deleted",
                    RemainingAttempts = MaxLoginAttempts - (attempts + 1)
                };
            }

            _cache.Remove(cacheKey);
            _cache.Remove(lockoutKey);
            return new AuthResult { Success = true, RemainingAttempts = MaxLoginAttempts };
        }

        public LoginResponse GenerateToken(LoginRequest req)
        {
            if (!DBContext.Users.Any(x => x.Email == req.Email))
                throw new ArgumentException("User does not exist");

            var user = DBContext.Users.FirstOrDefault(x => x.Email == req.Email);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, user!.Email),
                new(ClaimTypes.Role, user.Role.Name)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
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
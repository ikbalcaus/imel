using Imel.Models.Auth;

namespace Imel.Interfaces
{
    public interface IAuthService
    {
        LoginResponse GenerateToken(string username);
        AuthResult ValidateUser(LoginRequest loginRequest);
        void AddUser(string email, string username, string password, string? role = null);
        string HashPassword(string password);
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int RemainingAttempts { get; set; }
    }
}
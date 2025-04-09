using Imel.Models.Auth;

namespace Imel.Interfaces
{
    public interface IAuthService
    {
        LoginResponse GenerateToken(string email);
        AuthResult ValidateUser(LoginRequest loginRequest);
        void AddUser(string email, string username, string password);
    }
}
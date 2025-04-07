using Imel.Models.Auth;

namespace Imel.Interfaces
{
    public interface IAuthService
    {
        LoginResponse GenerateToken(string username);
        AuthResult ValidateUser(LoginRequest loginRequest);
        void AddUser(string email, string username, string password, int roleId);
    }
}
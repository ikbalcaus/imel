using Imel.Models.Auth;

namespace Imel.Interfaces
{
    public interface IAuthService
    {
        LoginResponse GenerateToken(LoginRequest req);
        AuthResult ValidateUser(LoginRequest req);
        void AddUser(RegisterRequest req);
    }
}
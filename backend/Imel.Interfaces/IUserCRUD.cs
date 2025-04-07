using Imel.Database.Models;

namespace Imel.Interfaces
{
    public interface IUserCRUD
    {
        IEnumerable<User> GetAllUsers();
        User GetUserByEmail(string email);
        User UpdateUser(string email, string? username, string? password, Role? role, bool? isActive);
        bool DeleteUser(string email);
        bool ToggleUserStatus(string email);
    }
}
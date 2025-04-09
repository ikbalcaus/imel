using Imel.Database.Models;
using Imel.Models.User;

namespace Imel.Interfaces
{
    public interface IUsersService
    {
        IEnumerable<User> GetAllUsers();
        User GetUserById(int id);
        User CreateUpdateUser(int id, CreateUpdateUserRequest req);
        bool DeleteUser(int id);
        bool ToggleUserStatus(int id);
    }
}
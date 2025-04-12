using Imel.Database.Models;
using Imel.Models;
using Imel.Models.User;

namespace Imel.Interfaces
{
    public interface IUsersService
    {
        Pagination<User> GetUsers(int pageNumber = 1, int pageSize = 10);
        User GetUserById(int id);
        User CreateUpdateUser(int id, CreateUpdateUserRequest req, int modifiedByUserId);
        bool DeleteUser(int id, int modifiedByUserId);
    }
}
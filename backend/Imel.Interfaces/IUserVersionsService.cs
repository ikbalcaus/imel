using Imel.Database.Models;
using Imel.Models;
using Imel.Models.UserVersions;

namespace Imel.Interfaces
{
    public interface IUserVersionsService
    {
        public Pagination<UserVersion> GetUsersVersions(int userId, int pageNumber = 1, int pageSize = 10);
        public User RevertUserVersion(int userId, RevertUserVersionRequest req);
    }
}

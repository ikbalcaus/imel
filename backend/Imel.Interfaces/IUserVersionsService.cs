using Imel.Database.Models;
using Imel.Models.UserVersions;

namespace Imel.Interfaces
{
    public interface IUserVersionsService
    {
        public IEnumerable<UserVersion> GetUserVersions(int userId);
        public User RevertUserVersion(int userId, RevertUserVersionRequest req, int modifiedByUserId);
    }
}

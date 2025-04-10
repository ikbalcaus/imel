using Imel.Database.Models;
using Imel.Database;
using Imel.Interfaces;

namespace Imel.Services
{
    public class UserVersionsService : IUserVersionsService
    {
        public IEnumerable<UserVersion> GetUserVersions(int userId)
        {
            return DBContext.UserVersions.Where(v => v.UserId == userId).OrderByDescending(v => v.VersionNumber);
        }
    }
}
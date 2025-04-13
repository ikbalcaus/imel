using Imel.Database.Models;
using Imel.Database;
using Imel.Interfaces;
using Imel.Models.UserVersions;
using Imel.Models;

namespace Imel.Services
{
    public class UserVersionsService : IUserVersionsService
    {
        public Pagination<UserVersion> GetUsersVersions(int userId, int pageNumber = 1, int pageSize = 10)
        {
            var query = DBContext.UserVersions.Where(x => x.UserId == userId).OrderByDescending(x => x.VersionNumber).AsQueryable();
            var totalCount = query.Count();
            var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new Pagination<UserVersion>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

        public User RevertUserVersion(int userId, RevertUserVersionRequest req)
        {
            var user = DBContext.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            var version = DBContext.UserVersions.FirstOrDefault(x => x.UserId == userId && x.VersionNumber == req.VersionNumber);
            var versionData = version!.UserData;
            var revertedUser = Helpers.CloneUser(versionData);

            Helpers.CreateAuditLog("user", userId, $"REVERT to {req.VersionNumber}", user, versionData);
            DBContext.Users[userId - 1] = revertedUser;
            Helpers.CreateUserVersion(revertedUser, $"REVERT to {req.VersionNumber}");

            return revertedUser;
        }
    }
}
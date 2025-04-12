using Imel.Database.Models;
using Imel.Database;
using Imel.Interfaces;
using Imel.Models.UserVersions;
using System;

namespace Imel.Services
{
    public class UserVersionsService : IUserVersionsService
    {
        public IEnumerable<UserVersion> GetUserVersions(int userId)
        {
            return DBContext.UserVersions.Where(v => v.UserId == userId).OrderByDescending(v => v.VersionNumber);
        }

        public User RevertUserVersion(int userId, RevertUserVersionRequest req, int modifiedByUserId)
        {
            if (DBContext.Users.FirstOrDefault(x => x.Id == userId) == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            var userData = DBContext.UserVersions.FirstOrDefault(x => x.UserId == userId && x.VersionNumber == req.VersionNumber)!.UserData;
            DBContext.Users[userId - 1] = Helpers.CloneUser(userData);

            Helpers.CreateUserVersion(userData, $"REVERT to {req.VersionNumber}", modifiedByUserId);
            return userData;
        }
    }
}
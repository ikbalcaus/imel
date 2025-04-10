using Imel.Database.Models;

namespace Imel.Interfaces
{
    public interface IUserVersionsService
    {
        public IEnumerable<UserVersion> GetUserVersions(int userId);
    }
}

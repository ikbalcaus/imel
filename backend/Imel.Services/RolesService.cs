using Imel.Database.Models;
using Imel.Database;
using Imel.Interfaces;

namespace Imel.Services
{
    public class RolesService : IRolesService
    {
        public IEnumerable<Role> GetAllRoles()
        {
            return DBContext.Roles.ToList();
        }
    }
}
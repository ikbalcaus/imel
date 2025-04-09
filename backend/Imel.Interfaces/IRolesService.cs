using Imel.Database.Models;

namespace Imel.Interfaces
{
    public interface IRolesService
    {
        IEnumerable<Role> GetAllRoles();
    }
}
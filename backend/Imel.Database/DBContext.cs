using Imel.Database.Models;

namespace Imel.Database
{
    public static class DBContext
    {
        public static readonly Dictionary<int, User> Users = new();
        public static readonly List<UserVersion> UserVersions = new();
        public static List<Role> Roles => new()
        {
            new Role { Id = 1, Name = "User" },
            new Role { Id = 2, Name = "Admin" }
        };
    }
}
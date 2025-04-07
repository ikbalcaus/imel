using Imel.Database.Models;
using System.Data;

namespace Imel.Database
{
    public static class DBContext
    {
        public static readonly Dictionary<string, User> Users = new();
        public static List<Role> Roles => new()
        {
            new Role { Id = 1, Name = "User" },
            new Role { Id = 2, Name = "Admin" }
        };
    }
}
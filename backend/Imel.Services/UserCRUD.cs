using Imel.Database;
using Imel.Database.Models;
using Imel.Interfaces;

namespace Imel.Services
{
    public class UserCRUD : IUserCRUD
    {
        public IEnumerable<User> GetAllUsers()
        {
            return DBContext.Users.Values.ToList();
        }

        public User GetUserByEmail(string email)
        {
            return DBContext.Users.TryGetValue(email, out var user)
                ? user
                : throw new KeyNotFoundException("User not found");
        }

        public User UpdateUser(string email, string? username, string? password, Role? role, bool? isActive)
        {
            if (!DBContext.Users.TryGetValue(email, out var user))
                throw new KeyNotFoundException("User not found");

            if (!string.IsNullOrWhiteSpace(username))
            {
                if (DBContext.Users.Values.Any(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                    u.Email != email))
                {
                    throw new ArgumentException("Username is already taken");
                }
                user.Username = username;
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                if (password.Length < 8)
                    throw new ArgumentException("Password must be at least 8 characters");
                user.PasswordHash = Helpers.HashPassword(password);
            }

            if (role != null)
                user.Role = role;

            if (isActive.HasValue)
                user.IsActive = isActive.Value;

            user.LastModified = DateTime.UtcNow;
            return user;
        }

        public bool DeleteUser(string email)
        {
            if (email == "admin@example.com")
                throw new InvalidOperationException("Cannot delete admin user");

            return DBContext.Users.Remove(email);
        }

        public bool ToggleUserStatus(string email)
        {
            if (!DBContext.Users.TryGetValue(email, out var user))
                throw new KeyNotFoundException("User not found");

            user.IsActive = !user.IsActive;
            user.LastModified = DateTime.UtcNow;
            return user.IsActive;
        }
    }
}
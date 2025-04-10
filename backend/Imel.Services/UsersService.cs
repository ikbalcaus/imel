using Imel.Database;
using Imel.Database.Models;
using Imel.Interfaces;
using Imel.Models.User;
using System;

namespace Imel.Services
{
    public class UsersService : IUsersService
    {
        public IEnumerable<User> GetAllUsers()
        {
            return DBContext.Users.Values.ToList();
        }

        public User GetUserById(int id)
        {
            return DBContext.Users.TryGetValue(id, out var user)
                ? user
                : throw new KeyNotFoundException("User not found");
        }

        public User CreateUpdateUser(int id, CreateUpdateUserRequest req)
        {
            User user;
            string action;

            if (id == 0)
            {
                if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                {
                    throw new ArgumentException("Email, Username, or Password are empty");
                }
                if (DBContext.Users.Values.Any(x => x.Email.Equals(req.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException("Email is already taken");
                }
                if (DBContext.Users.Values.Any(x => x.Username.Equals(req.Username, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException("Username is already taken");
                }
                if (!Helpers.IsValidEmail(req.Email!))
                {
                    throw new ArgumentException("Invalid email format");
                }
                if (req.Password.Length < 8)
                {
                    throw new ArgumentException("Password must be at least 8 characters");
                }

                user = new User
                {
                    Id = DBContext.Users.Any() ? DBContext.Users.Max(x => x.Value.Id) + 1 : 1,
                    Email = req.Email!,
                    Username = req.Username!,
                    PasswordHash = Helpers.HashPassword(req.Password!),
                    RoleId = req.RoleId ?? 1,
                    Role = DBContext.Roles.FirstOrDefault(x => x.Id == req.RoleId || x.Id == 1)!,
                    IsActive = req.IsActive ?? true,
                    LastModified = DateTime.UtcNow
                };

                DBContext.Users[id] = user;
                action = "CREATE";
            }

            else
            {
                if (!DBContext.Users.TryGetValue(id, out var userData))
                {
                    throw new KeyNotFoundException("User not found");
                }
                user = userData;

                if (!string.IsNullOrWhiteSpace(req.Email) && !req.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    if (DBContext.Users.Values.Any(x => x.Email.Equals(req.Email, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new ArgumentException("Email is already taken");
                    }
                    user.Email = req.Email;
                }
                if (!string.IsNullOrWhiteSpace(req.Username) && !req.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase))
                {
                    if (DBContext.Users.Values.Any(x => x.Username.Equals(req.Username, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new ArgumentException("Username is already taken");
                    }
                    user.Username = req.Username;
                }
                if (!string.IsNullOrWhiteSpace(req.Password))
                {
                    if (req.Password.Length > 0 && req.Password.Length < 8)
                    {
                        throw new ArgumentException("Password must be at least 8 characters");
                    }
                    user.PasswordHash = Helpers.HashPassword(req.Password);
                }

                user.RoleId = req.RoleId ?? 1;
                user.Role = DBContext.Roles.FirstOrDefault(x => x.Id == req.RoleId || x.Id == 1)!;
                user.IsActive = req.IsActive ?? true;
                user.LastModified = DateTime.UtcNow;
                action = "UPDATE";
            }

            Helpers.CreateUserVersion(user, action);
            return user;
        }

        public bool DeleteUser(int id)
        {
            if (!DBContext.Users.TryGetValue(id, out var user)) return false;
            Helpers.CreateUserVersion(user, "DELETE");
            return DBContext.Users.Remove(id);
        }

        public bool ToggleUserStatus(int id)
        {
            if (!DBContext.Users.TryGetValue(id, out var user))
                throw new KeyNotFoundException("User not found");

            user.IsActive = !user.IsActive;
            user.LastModified = DateTime.UtcNow;
            return user.IsActive;
        }
    }
}
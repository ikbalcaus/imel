using Imel.Database;
using Imel.Database.Models;
using Imel.Interfaces;
using Imel.Models;
using Imel.Models.User;
using System;
using static System.Collections.Specialized.BitVector32;

namespace Imel.Services
{
    public class UsersService : IUsersService
    {
        public Pagination<User> GetUsers(int pageNumber = 1, int pageSize = 10)
        {
            var query = DBContext.Users.AsQueryable();

            var totalCount = query.Count();
            var items = query
                .OrderBy(u => u.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new Pagination<User>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

        public User GetUserById(int id)
        {
            var user = DBContext.Users.FirstOrDefault(x => x.Id == id);
            return user ?? throw new KeyNotFoundException("User not found");
        }

        public User CreateUpdateUser(int id, CreateUpdateUserRequest req, int modifiedByUserId)
        {
            User user;

            if (id == 0)
            {
                if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                {
                    throw new ArgumentException("Email, Username, or Password are empty");
                }
                if (DBContext.Users.Any(x => x.Email.Equals(req.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException("Email is already taken");
                }
                if (DBContext.Users.Any(x => x.Username.Equals(req.Username, StringComparison.OrdinalIgnoreCase)))
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
                    Id = DBContext.Users.Any() ? DBContext.Users.Max(x => x.Id) + 1 : 1,
                    Email = req.Email!,
                    Username = req.Username!,
                    PasswordHash = Helpers.HashPassword(req.Password!),
                    RoleId = req.RoleId ?? 1,
                    Role = DBContext.Roles.FirstOrDefault(x => x.Id == req.RoleId || x.Id == 1)!,
                    IsActive = req.IsActive ?? true
                };
                Helpers.CreateUserVersion(user, "CREATE", modifiedByUserId);
                DBContext.Users[id] = user;
            }

            else
            {
                var userData = DBContext.Users.FirstOrDefault(x => x.Id == id);
                if (userData == null)
                {
                    throw new KeyNotFoundException("User not found");
                }
                user = userData;

                if (!string.IsNullOrWhiteSpace(req.Email) && !req.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    if (DBContext.Users.Any(x => x.Email.Equals(req.Email, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new ArgumentException("Email is already taken");
                    }
                    user.Email = req.Email;
                }
                if (!string.IsNullOrWhiteSpace(req.Username) && !req.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase))
                {
                    if (DBContext.Users.Any(x => x.Username.Equals(req.Username, StringComparison.OrdinalIgnoreCase)))
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

                user.RoleId = req.RoleId ?? DBContext.Users.FirstOrDefault(x => x.Id == id)!.RoleId;
                user.Role = DBContext.Roles.FirstOrDefault(x => x.Id == req.RoleId)!;
                user.IsActive = req.IsActive ?? DBContext.Users.FirstOrDefault(x => x.Id == id)!.IsActive;
                user.IsDeleted = false;

                Helpers.CreateUserVersion(userData, "UPDATE", modifiedByUserId);
            }
            return user;
        }

        public bool DeleteUser(int id, int modifiedByUserId)
        {
            var user = DBContext.Users.FirstOrDefault(x => x.Id == id);
            if (user == null) return false;
            Helpers.CreateUserVersion(user, "DELETE", modifiedByUserId);
            return DBContext.Users.FirstOrDefault(x => x.Id == id)!.IsDeleted = true;
        }
    }
}
using Imel.Database;
using Imel.Database.Models;
using Imel.Interfaces;
using Imel.Models;
using Imel.Models.User;

namespace Imel.Services
{
    public class UsersService : IUsersService
    {
        public Pagination<User> GetUsers(int pageNumber = 1, int pageSize = 10)
        {
            var query = DBContext.Users.AsQueryable();

            var totalCount = query.Count();
            var items = query.OrderBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

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

        public User CreateUpdateUser(int id, CreateUpdateUserRequest req)
        {
            if (id == 0)
            {
                var userData = DBContext.Users.FirstOrDefault(x => x.Id == id);

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

                var newId = DBContext.Users.Any() ? DBContext.Users.Max(x => x.Id) + 1 : 1;
                userData = new User
                {
                    Id = newId,
                    Email = req.Email!,
                    Username = req.Username!,
                    PasswordHash = Helpers.HashPassword(req.Password!),
                    RoleId = req.RoleId ?? 1,
                    Role = DBContext.Roles.FirstOrDefault(x => x.Id == req.RoleId || x.Id == 1)!,
                    IsActive = req.IsActive ?? true
                };
                DBContext.Users.Add(userData);
                Helpers.CreateUserVersion(userData, "CREATE");
                Helpers.CreateAuditLog("User", userData.Id, "CREATE", "", userData);
                return userData;
            }

            else
            {
                var newUserData = DBContext.Users.FirstOrDefault(x => x.Id == id);
                if (newUserData == null)
                {
                    throw new KeyNotFoundException("User not found");
                }
                var oldUserData = Helpers.CloneUser(newUserData);

                if (!string.IsNullOrWhiteSpace(req.Email) && !req.Email.Equals(newUserData.Email, StringComparison.OrdinalIgnoreCase))
                {
                    if (DBContext.Users.Any(x => x.Email.Equals(req.Email, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new ArgumentException("Email is already taken");
                    }
                    newUserData.Email = req.Email;
                }
                if (!string.IsNullOrWhiteSpace(req.Username) && !req.Username.Equals(newUserData.Username, StringComparison.OrdinalIgnoreCase))
                {
                    if (DBContext.Users.Any(x => x.Username.Equals(req.Username, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new ArgumentException("Username is already taken");
                    }
                    newUserData.Username = req.Username;
                }
                if (!string.IsNullOrWhiteSpace(req.Password))
                {
                    if (req.Password.Length > 0 && req.Password.Length < 8)
                    {
                        throw new ArgumentException("Password must be at least 8 characters");
                    }
                    newUserData.PasswordHash = Helpers.HashPassword(req.Password);
                }

                newUserData.RoleId = req.RoleId ?? DBContext.Users.FirstOrDefault(x => x.Id == id)!.RoleId;
                newUserData.Role = DBContext.Roles.FirstOrDefault(x => x.Id == req.RoleId)!;
                newUserData.IsActive = req.IsActive ?? DBContext.Users.FirstOrDefault(x => x.Id == id)!.IsActive;
                newUserData.IsDeleted = false;

                Helpers.CreateUserVersion(newUserData, "UPDATE");
                Helpers.CreateAuditLog("User", newUserData.Id, "UPDATE", oldUserData, newUserData);
                return newUserData;
            }
        }

        public bool DeleteUser(int id)
        {
            var user = DBContext.Users.FirstOrDefault(x => x.Id == id);
            if (user == null) return false;
            Helpers.CreateUserVersion(user, "DELETE");
            Helpers.CreateAuditLog("User", id, "DELETE", user, "");
            return DBContext.Users.FirstOrDefault(x => x.Id == id)!.IsDeleted = true;
        }
    }
}
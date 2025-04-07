using Imel.Database.Models;
using System.ComponentModel.DataAnnotations;

namespace Imel.Models.User
{
    public class UpdateUserRequest
    {
        public string? Username { get; set; }

        [MinLength(8)]
        public string? Password { get; set; }

        public Role? Role { get; set; }

        public bool? IsActive { get; set; }
    }
}

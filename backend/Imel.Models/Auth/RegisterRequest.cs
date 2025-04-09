using System.ComponentModel.DataAnnotations;

namespace Imel.Models.Auth
{
    public class RegisterRequest
    {
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;

        [MinLength(8)]
        public string Password { get; set; } = null!;
    }
}
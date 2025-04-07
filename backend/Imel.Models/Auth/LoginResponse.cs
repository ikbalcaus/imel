namespace Imel.Models.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public string Message { get; set; } = null!;
    }
}
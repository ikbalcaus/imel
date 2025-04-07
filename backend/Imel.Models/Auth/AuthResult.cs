namespace Imel.Models.Auth
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int RemainingAttempts { get; set; }
    }
}

namespace Imel.Database.Models
{
    public class UserVersion
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User UserData { get; set; } = null!;
        public int VersionNumber { get; set; }
        public DateTime ModifiedAt { get; set; } = DateTime.Now;
        public string Action { get; set; } = null!; // "CREATE", "UPDATE", "DELETE", "REVERT"
        public User ModifiedByUser { get; set; } = null!;
    }
}
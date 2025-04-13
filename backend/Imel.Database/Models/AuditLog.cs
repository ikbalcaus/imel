namespace Imel.Database.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = null!;
        public int EntityId { get; set; }
        public string Action { get; set; } = null!;
        public string OldValues { get; set; } = null!;
        public string NewValues { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string ModifiedByUser { get; set; } = null!;
    }
}
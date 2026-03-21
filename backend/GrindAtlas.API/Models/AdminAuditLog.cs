namespace GrindAtlas.API.Models;

public class AdminAuditLog
{
    public int Id { get; set; }
    public string ActorId { get; set; } = "";
    public string ActorEmail { get; set; } = "";
    public string Action { get; set; } = "";        // e.g. "SuspendUser", "DeleteLog", "MergeCoffee"
    public string EntityType { get; set; } = "";    // e.g. "User", "GrindLog", "Coffee", "Grinder"
    public string? EntityId { get; set; }
    public string? OldValue { get; set; }           // JSON snapshot before change
    public string? NewValue { get; set; }           // JSON snapshot after change
    public string? Notes { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

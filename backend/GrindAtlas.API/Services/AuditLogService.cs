using GrindAtlas.API.Data;
using GrindAtlas.API.Models;

namespace GrindAtlas.API.Services;

public interface IAuditLogService
{
    Task LogAsync(
        string actorId,
        string actorEmail,
        string action,
        string entityType,
        string? entityId = null,
        string? oldValue = null,
        string? newValue = null,
        string? notes = null,
        string? ipAddress = null);
}

public class AuditLogService(AppDbContext ctx) : IAuditLogService
{
    public async Task LogAsync(
        string actorId,
        string actorEmail,
        string action,
        string entityType,
        string? entityId = null,
        string? oldValue = null,
        string? newValue = null,
        string? notes = null,
        string? ipAddress = null)
    {
        ctx.AdminAuditLogs.Add(new AdminAuditLog
        {
            ActorId    = actorId,
            ActorEmail = actorEmail,
            Action     = action,
            EntityType = entityType,
            EntityId   = entityId,
            OldValue   = oldValue,
            NewValue   = newValue,
            Notes      = notes,
            IpAddress  = ipAddress,
            Timestamp  = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();
    }
}

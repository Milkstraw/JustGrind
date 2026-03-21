using GrindAtlas.API.Data;
using GrindAtlas.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GrindAtlas.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(AppDbContext ctx) : ControllerBase
{
    protected string ActorId    => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    protected string ActorEmail => User.FindFirstValue(ClaimTypes.Email)!;

    // ── Audit Log ─────────────────────────────────────────────────────────────

    [HttpGet("audit-log")]
    public IActionResult GetAuditLog(
        [FromQuery] string? actor,
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 50)
    {
        if (page < 1)     page     = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 200) pageSize = 200;

        var query = ctx.AdminAuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(actor))
            query = query.Where(l => l.ActorEmail.Contains(actor));
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(l => l.Action.Contains(action));
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(l => l.EntityType == entityType);
        if (from.HasValue)
            query = query.Where(l => l.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.Timestamp <= to.Value);

        var total = query.Count();
        var items = query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AdminAuditLogResponse(
                l.Id, l.ActorId, l.ActorEmail, l.Action,
                l.EntityType, l.EntityId, l.OldValue, l.NewValue,
                l.Notes, l.IpAddress, l.Timestamp))
            .ToList();

        return Ok(new { total, page, pageSize, items });
    }
}

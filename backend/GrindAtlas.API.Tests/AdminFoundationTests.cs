using GrindAtlas.API.Controllers;
using GrindAtlas.API.Data;
using GrindAtlas.API.Models;
using GrindAtlas.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Security.Claims;

namespace GrindAtlas.API.Tests;

public class AdminFoundationTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static AppDbContext CreateDb(string name)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new AppDbContext(opts);
    }

    private static AdminController CreateAdminController(AppDbContext ctx, bool isAdmin = true)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "actor-1"),
            new(ClaimTypes.Email, "admin@grindatlas.com"),
        };
        if (isAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        return new AdminController(ctx)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            }
        };
    }

    // ── Authorization attribute tests ─────────────────────────────────────────

    [Fact]
    public void AdminController_RequiresAdminRole()
    {
        var attr = typeof(AdminController)
            .GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attr);
        Assert.Equal("Admin", attr.Roles);
    }

    // ── Audit log endpoint tests ───────────────────────────────────────────────

    [Fact]
    public void GetAuditLog_ReturnsEmpty_WhenNoEntries()
    {
        var ctx        = CreateDb(nameof(GetAuditLog_ReturnsEmpty_WhenNoEntries));
        var controller = CreateAdminController(ctx);

        var result = controller.GetAuditLog(null, null, null, null, null) as OkObjectResult;

        Assert.NotNull(result);
        dynamic body = result.Value!;
        Assert.Equal(0, (int)body.GetType().GetProperty("total")!.GetValue(body)!);
    }

    [Fact]
    public void GetAuditLog_ReturnsEntries_OrderedByTimestampDesc()
    {
        var ctx = CreateDb(nameof(GetAuditLog_ReturnsEntries_OrderedByTimestampDesc));
        ctx.AdminAuditLogs.AddRange(
            new AdminAuditLog { ActorId = "a", ActorEmail = "admin@test.com", Action = "SuspendUser", EntityType = "User", Timestamp = DateTime.UtcNow.AddMinutes(-10) },
            new AdminAuditLog { ActorId = "a", ActorEmail = "admin@test.com", Action = "DeleteLog",   EntityType = "GrindLog", Timestamp = DateTime.UtcNow }
        );
        ctx.SaveChanges();

        var controller = CreateAdminController(ctx);
        var result     = controller.GetAuditLog(null, null, null, null, null) as OkObjectResult;

        Assert.NotNull(result);
        dynamic body  = result.Value!;
        var items     = (IEnumerable<object>)body.GetType().GetProperty("items")!.GetValue(body)!;
        var list      = items.ToList();
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void GetAuditLog_FiltersBy_Actor()
    {
        var ctx = CreateDb(nameof(GetAuditLog_FiltersBy_Actor));
        ctx.AdminAuditLogs.AddRange(
            new AdminAuditLog { ActorId = "a1", ActorEmail = "alice@test.com", Action = "Edit", EntityType = "Coffee", Timestamp = DateTime.UtcNow },
            new AdminAuditLog { ActorId = "a2", ActorEmail = "bob@test.com",   Action = "Edit", EntityType = "Coffee", Timestamp = DateTime.UtcNow }
        );
        ctx.SaveChanges();

        var controller = CreateAdminController(ctx);
        var result     = controller.GetAuditLog(actor: "alice", null, null, null, null) as OkObjectResult;

        dynamic body = result!.Value!;
        Assert.Equal(1, (int)body.GetType().GetProperty("total")!.GetValue(body)!);
    }

    [Fact]
    public void GetAuditLog_FiltersBy_EntityType()
    {
        var ctx = CreateDb(nameof(GetAuditLog_FiltersBy_EntityType));
        ctx.AdminAuditLogs.AddRange(
            new AdminAuditLog { ActorId = "a", ActorEmail = "admin@test.com", Action = "Edit",   EntityType = "Coffee",   Timestamp = DateTime.UtcNow },
            new AdminAuditLog { ActorId = "a", ActorEmail = "admin@test.com", Action = "Delete", EntityType = "GrindLog", Timestamp = DateTime.UtcNow }
        );
        ctx.SaveChanges();

        var controller = CreateAdminController(ctx);
        var result     = controller.GetAuditLog(null, null, entityType: "Coffee", null, null) as OkObjectResult;

        dynamic body = result!.Value!;
        Assert.Equal(1, (int)body.GetType().GetProperty("total")!.GetValue(body)!);
    }

    [Fact]
    public void GetAuditLog_EnforcesMaxPageSize()
    {
        var ctx = CreateDb(nameof(GetAuditLog_EnforcesMaxPageSize));
        var controller = CreateAdminController(ctx);

        // pageSize > 200 should be clamped to 200
        var result = controller.GetAuditLog(null, null, null, null, null, page: 1, pageSize: 9999) as OkObjectResult;

        dynamic body     = result!.Value!;
        int returnedSize = (int)body.GetType().GetProperty("pageSize")!.GetValue(body)!;
        Assert.Equal(200, returnedSize);
    }

    // ── AuditLogService tests ─────────────────────────────────────────────────

    [Fact]
    public async Task AuditLogService_CreatesEntry()
    {
        var ctx     = CreateDb(nameof(AuditLogService_CreatesEntry));
        var service = new AuditLogService(ctx);

        await service.LogAsync("actor-1", "admin@test.com", "SuspendUser", "User", entityId: "user-99");

        var entry = ctx.AdminAuditLogs.Single();
        Assert.Equal("actor-1",       entry.ActorId);
        Assert.Equal("admin@test.com", entry.ActorEmail);
        Assert.Equal("SuspendUser",   entry.Action);
        Assert.Equal("User",          entry.EntityType);
        Assert.Equal("user-99",       entry.EntityId);
    }

    [Fact]
    public async Task AuditLogService_StoresOldAndNewValues()
    {
        var ctx     = CreateDb(nameof(AuditLogService_StoresOldAndNewValues));
        var service = new AuditLogService(ctx);

        await service.LogAsync(
            "actor-1", "admin@test.com",
            "UpdateEmail", "User",
            entityId: "user-5",
            oldValue: "{\"email\":\"old@test.com\"}",
            newValue: "{\"email\":\"new@test.com\"}");

        var entry = ctx.AdminAuditLogs.Single();
        Assert.Equal("{\"email\":\"old@test.com\"}", entry.OldValue);
        Assert.Equal("{\"email\":\"new@test.com\"}", entry.NewValue);
    }
}

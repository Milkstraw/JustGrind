using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GrindAtlas.API.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connStr = BuildConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connStr)
            .Options;
        return new AppDbContext(options);
    }

    private static string BuildConnectionString()
    {
        // Render (and most PaaS) expose DATABASE_URL as postgres://user:pass@host:port/db
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(databaseUrl))
            return ParseDatabaseUrl(databaseUrl);

        // Fall back to individual vars (same as Program.cs)
        var host     = Environment.GetEnvironmentVariable("DB_HOST")     ?? throw new InvalidOperationException("Set DATABASE_URL or DB_HOST before running EF commands.");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new InvalidOperationException("Set DATABASE_URL or DB_PASSWORD before running EF commands.");
        var name     = Environment.GetEnvironmentVariable("DB_NAME")     ?? "neondb";
        var user     = Environment.GetEnvironmentVariable("DB_USER")     ?? "neondb_owner";
        return $"Host={host};Database={name};Username={user};Password={password};SSL Mode=Require";
    }

    // Converts postgres://user:pass@host:port/db  →  Npgsql connection string
    private static string ParseDatabaseUrl(string url)
    {
        var uri      = new Uri(url);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user     = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var host     = uri.Host;
        var port     = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');
        return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require";
    }
}

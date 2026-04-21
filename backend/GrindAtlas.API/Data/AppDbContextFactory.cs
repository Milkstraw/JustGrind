using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GrindAtlas.API.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connStr =
            Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? "Host=localhost;Database=grindatlas_dev;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connStr)
            .Options;

        return new AppDbContext(options);
    }
}

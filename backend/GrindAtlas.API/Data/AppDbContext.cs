using GrindAtlas.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GrindAtlas.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Coffee> Coffees => Set<Coffee>();
    public DbSet<Grinder> Grinders => Set<Grinder>();
    public DbSet<GrinderCalibration> GrinderCalibrations => Set<GrinderCalibration>();
    public DbSet<GrindLog> GrindLogs => Set<GrindLog>();
    public DbSet<BrewRecipe> BrewRecipes => Set<BrewRecipe>();
    public DbSet<BrewRecipeStep> BrewRecipeSteps => Set<BrewRecipeStep>();
    public DbSet<GrindEstimate> GrindEstimates => Set<GrindEstimate>();
    public DbSet<UserCoffee> UserCoffees => Set<UserCoffee>();
    public DbSet<UserGrinder> UserGrinders => Set<UserGrinder>();
    public DbSet<UserBrewMethod> UserBrewMethods => Set<UserBrewMethod>();
    public DbSet<CoffeeBag> CoffeeBags => Set<CoffeeBag>();
    public DbSet<AdminAuditLog> AdminAuditLogs => Set<AdminAuditLog>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb); // required for Identity tables
        mb.Entity<Coffee>().Property(c => c.RoastLevel).HasPrecision(3, 1);
        mb.Entity<GrindLog>().Property(l => l.NgiNormalized).HasPrecision(5, 2);
        mb.Entity<GrindLog>().Property(l => l.NativeSetting).HasPrecision(6, 2);
        mb.Entity<GrinderCalibration>().Property(c => c.NgiValue).HasPrecision(5, 2);
        mb.Entity<GrindEstimate>().Property(e => e.AvgSimilarityScore).HasPrecision(4, 3);
        mb.Entity<GrinderCalibration>().Property(c => c.NativeSetting).HasPrecision(6, 2);
        mb.Entity<GrindEstimate>().Property(e => e.ConfidenceScore).HasPrecision(4, 3);
        mb.Entity<CoffeeBag>().Property(b => b.BagWeightG).HasPrecision(6, 1);
        mb.Entity<UserCoffee>().HasIndex(uc => new { uc.UserId, uc.CoffeeId }).IsUnique();
        mb.Entity<UserGrinder>().HasIndex(ug => new { ug.UserId, ug.GrinderId }).IsUnique();
        mb.Entity<UserBrewMethod>().HasIndex(ubm => new { ubm.UserId, ubm.BrewMethod }).IsUnique();
    }
}

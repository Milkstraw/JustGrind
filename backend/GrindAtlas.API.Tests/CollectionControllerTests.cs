using GrindAtlas.API.Controllers;
using GrindAtlas.API.Data;
using GrindAtlas.API.DTOs;
using GrindAtlas.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GrindAtlas.API.Tests;

public class CollectionControllerTests
{
    private static AppDbContext CreateDb(string name)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new AppDbContext(opts);
    }

    private static CollectionController CreateController(AppDbContext ctx, string userId = "user-1")
    {
        var claims = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
        ], "test"));

        return new CollectionController(ctx)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claims }
            }
        };
    }

    private static Coffee SeedCoffee(AppDbContext ctx, int id = 1)
    {
        var coffee = new Coffee
        {
            Id = id, Name = $"Coffee {id}", ProcessingMethod = ProcessingMethod.Washed,
            Species = Species.Arabica, RoastLevel = 2.5m, IsBlend = false,
            IsActive = true, CreatedAt = DateTime.UtcNow,
        };
        ctx.Coffees.Add(coffee);
        ctx.SaveChanges();
        return coffee;
    }

    private static Grinder SeedGrinder(AppDbContext ctx, int id = 1)
    {
        var grinder = new Grinder
        {
            Id = id, Brand = "Baratza", Model = "Encore",
            GrindType = GrindType.Stepped, BurrType = BurrType.Conical,
            ScaleType = ScaleType.Numeric, ScaleMin = 1, ScaleMax = 40,
            IsVerified = true,
        };
        ctx.Grinders.Add(grinder);
        ctx.SaveChanges();
        return grinder;
    }

    // ── My Shelf ──────────────────────────────────────────────────────────────

    [Fact]
    public void AddToShelf_NewCoffee_ReturnsOk()
    {
        using var ctx = CreateDb(nameof(AddToShelf_NewCoffee_ReturnsOk));
        SeedCoffee(ctx);
        var ctrl = CreateController(ctx);

        var result = ctrl.AddToShelf(1);

        Assert.IsType<OkResult>(result);
        Assert.Single(ctx.UserCoffees);
    }

    [Fact]
    public void AddToShelf_Duplicate_IsIdempotent()
    {
        using var ctx = CreateDb(nameof(AddToShelf_Duplicate_IsIdempotent));
        SeedCoffee(ctx);
        var ctrl = CreateController(ctx);

        ctrl.AddToShelf(1);
        var result = ctrl.AddToShelf(1); // second call

        Assert.IsType<OkResult>(result);
        Assert.Single(ctx.UserCoffees); // still only one entry
    }

    [Fact]
    public void AddToShelf_CoffeeNotFound_ReturnsNotFound()
    {
        using var ctx = CreateDb(nameof(AddToShelf_CoffeeNotFound_ReturnsNotFound));
        var ctrl = CreateController(ctx);

        var result = ctrl.AddToShelf(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void RemoveFromShelf_ExistingEntry_RemovesAndReturnsNoContent()
    {
        using var ctx = CreateDb(nameof(RemoveFromShelf_ExistingEntry_RemovesAndReturnsNoContent));
        SeedCoffee(ctx);
        var ctrl = CreateController(ctx);
        ctrl.AddToShelf(1);

        var result = ctrl.RemoveFromShelf(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(ctx.UserCoffees);
    }

    [Fact]
    public void RemoveFromShelf_NotOnShelf_ReturnsNoContent()
    {
        using var ctx = CreateDb(nameof(RemoveFromShelf_NotOnShelf_ReturnsNoContent));
        var ctrl = CreateController(ctx);

        var result = ctrl.RemoveFromShelf(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void GetShelf_ReturnsOnlyCurrentUserItems()
    {
        using var ctx = CreateDb(nameof(GetShelf_ReturnsOnlyCurrentUserItems));
        SeedCoffee(ctx, 1);
        SeedCoffee(ctx, 2);

        var ctrl1 = CreateController(ctx, "user-1");
        var ctrl2 = CreateController(ctx, "user-2");
        ctrl1.AddToShelf(1);
        ctrl2.AddToShelf(2);

        var result = ctrl1.GetShelf() as OkObjectResult;
        var items = result!.Value as IEnumerable<UserCoffee>;

        Assert.Single(items!);
        Assert.Equal(1, items!.First().CoffeeId);
    }

    // ── My Setup — Grinders ───────────────────────────────────────────────────

    [Fact]
    public void AddGrinderToSetup_NewGrinder_ReturnsOk()
    {
        using var ctx = CreateDb(nameof(AddGrinderToSetup_NewGrinder_ReturnsOk));
        SeedGrinder(ctx);
        var ctrl = CreateController(ctx);

        var result = ctrl.AddGrinderToSetup(1);

        Assert.IsType<OkResult>(result);
        Assert.Single(ctx.UserGrinders);
    }

    [Fact]
    public void AddGrinderToSetup_Duplicate_IsIdempotent()
    {
        using var ctx = CreateDb(nameof(AddGrinderToSetup_Duplicate_IsIdempotent));
        SeedGrinder(ctx);
        var ctrl = CreateController(ctx);

        ctrl.AddGrinderToSetup(1);
        ctrl.AddGrinderToSetup(1);

        Assert.Single(ctx.UserGrinders);
    }

    [Fact]
    public void RemoveGrinderFromSetup_Existing_RemovesAndReturnsNoContent()
    {
        using var ctx = CreateDb(nameof(RemoveGrinderFromSetup_Existing_RemovesAndReturnsNoContent));
        SeedGrinder(ctx);
        var ctrl = CreateController(ctx);
        ctrl.AddGrinderToSetup(1);

        var result = ctrl.RemoveGrinderFromSetup(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(ctx.UserGrinders);
    }

    // ── My Setup — Brew Methods ───────────────────────────────────────────────

    [Fact]
    public void AddBrewMethodToSetup_New_ReturnsOk()
    {
        using var ctx = CreateDb(nameof(AddBrewMethodToSetup_New_ReturnsOk));
        var ctrl = CreateController(ctx);

        var result = ctrl.AddBrewMethodToSetup(BrewMethod.PourOver);

        Assert.IsType<OkResult>(result);
        Assert.Single(ctx.UserBrewMethods);
    }

    [Fact]
    public void AddBrewMethodToSetup_Duplicate_IsIdempotent()
    {
        using var ctx = CreateDb(nameof(AddBrewMethodToSetup_Duplicate_IsIdempotent));
        var ctrl = CreateController(ctx);

        ctrl.AddBrewMethodToSetup(BrewMethod.PourOver);
        ctrl.AddBrewMethodToSetup(BrewMethod.PourOver);

        Assert.Single(ctx.UserBrewMethods);
    }

    [Fact]
    public void GetSetupBrewMethods_ReturnsOnlyCurrentUser()
    {
        using var ctx = CreateDb(nameof(GetSetupBrewMethods_ReturnsOnlyCurrentUser));
        var ctrl1 = CreateController(ctx, "user-1");
        var ctrl2 = CreateController(ctx, "user-2");

        ctrl1.AddBrewMethodToSetup(BrewMethod.PourOver);
        ctrl1.AddBrewMethodToSetup(BrewMethod.Espresso);
        ctrl2.AddBrewMethodToSetup(BrewMethod.FrenchPress);

        var result = ctrl1.GetSetupBrewMethods() as OkObjectResult;
        var items = result!.Value as IEnumerable<UserBrewMethod>;

        Assert.Equal(2, items!.Count());
    }

    // ── Bag Tracking ─────────────────────────────────────────────────────────

    [Fact]
    public void OpenBag_ValidCoffee_CreatesBag()
    {
        using var ctx = CreateDb(nameof(OpenBag_ValidCoffee_CreatesBag));
        SeedCoffee(ctx);
        var ctrl = CreateController(ctx);

        var result = ctrl.OpenBag(new OpenCoffeeBagRequest(1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), 250m, null));

        Assert.IsType<OkObjectResult>(result);
        Assert.Single(ctx.CoffeeBags);
    }

    [Fact]
    public void OpenBag_CoffeeNotFound_ReturnsNotFound()
    {
        using var ctx = CreateDb(nameof(OpenBag_CoffeeNotFound_ReturnsNotFound));
        var ctrl = CreateController(ctx);

        var result = ctrl.OpenBag(new OpenCoffeeBagRequest(999, null, null, null));

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void GetFreshness_WashedCoffee_PeakDay7To21()
    {
        using var ctx = CreateDb(nameof(GetFreshness_WashedCoffee_PeakDay7To21));
        var coffee = SeedCoffee(ctx);
        var ctrl = CreateController(ctx);

        // Coffee roasted 14 days ago → should be "Peak" for Washed (7–21 days)
        ctrl.OpenBag(new OpenCoffeeBagRequest(1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-14)), 250m, null));
        var bag = ctx.CoffeeBags.First();

        var result = ctrl.GetFreshness(bag.Id) as OkObjectResult;
        var freshness = result!.Value as FreshnessInfo;

        Assert.Equal("Peak", freshness!.FreshnessStatus);
        Assert.Equal(7, freshness.PeakStartDay);
        Assert.Equal(21, freshness.PeakEndDay);
    }

    [Fact]
    public void GetFreshness_TooFreshCoffee_ReturnsCorrectStatus()
    {
        using var ctx = CreateDb(nameof(GetFreshness_TooFreshCoffee_ReturnsCorrectStatus));
        SeedCoffee(ctx);
        var ctrl = CreateController(ctx);

        // Roasted only 3 days ago → "Too Fresh"
        ctrl.OpenBag(new OpenCoffeeBagRequest(1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)), 250m, null));
        var bag = ctx.CoffeeBags.First();

        var result = ctrl.GetFreshness(bag.Id) as OkObjectResult;
        var freshness = result!.Value as FreshnessInfo;

        Assert.Equal("Too Fresh", freshness!.FreshnessStatus);
    }

    [Fact]
    public void GetFreshness_PastPeakCoffee_ReturnsCorrectStatus()
    {
        using var ctx = CreateDb(nameof(GetFreshness_PastPeakCoffee_ReturnsCorrectStatus));
        SeedCoffee(ctx);
        var ctrl = CreateController(ctx);

        // Roasted 60 days ago → "Past Peak"
        ctrl.OpenBag(new OpenCoffeeBagRequest(1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-60)), 250m, null));
        var bag = ctx.CoffeeBags.First();

        var result = ctrl.GetFreshness(bag.Id) as OkObjectResult;
        var freshness = result!.Value as FreshnessInfo;

        Assert.Equal("Past Peak", freshness!.FreshnessStatus);
    }

    [Fact]
    public void CloseBag_ExistingBag_RemovesIt()
    {
        using var ctx = CreateDb(nameof(CloseBag_ExistingBag_RemovesIt));
        SeedCoffee(ctx);
        var ctrl = CreateController(ctx);
        ctrl.OpenBag(new OpenCoffeeBagRequest(1, null, null, null));
        var bagId = ctx.CoffeeBags.First().Id;

        var result = ctrl.CloseBag(bagId);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(ctx.CoffeeBags);
    }

    [Fact]
    public void GetFreshness_OtherUser_ReturnsNotFound()
    {
        using var ctx = CreateDb(nameof(GetFreshness_OtherUser_ReturnsNotFound));
        SeedCoffee(ctx);
        var ctrl1 = CreateController(ctx, "user-1");
        var ctrl2 = CreateController(ctx, "user-2");

        ctrl1.OpenBag(new OpenCoffeeBagRequest(1, null, null, null));
        var bagId = ctx.CoffeeBags.First().Id;

        var result = ctrl2.GetFreshness(bagId);

        Assert.IsType<NotFoundResult>(result);
    }
}

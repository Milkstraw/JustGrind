using GrindAtlas.API.Controllers;
using GrindAtlas.API.Data;
using GrindAtlas.API.DTOs;
using GrindAtlas.API.Models;
using GrindAtlas.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GrindAtlas.API.Tests;

public class GrindLogControllerTests
{
    private static AppDbContext CreateDb(string name)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new AppDbContext(opts);
    }

    private static GrindLogsController CreateController(AppDbContext ctx, string userId = "user-1")
    {
        var claims = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
        ], "test"));

        var estimator = new GrindEstimatorService(ctx);

        return new GrindLogsController(ctx, estimator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claims }
            }
        };
    }

    private static void SeedCoffeeAndGrinder(AppDbContext ctx)
    {
        ctx.Coffees.Add(new Coffee
        {
            Id = 1, Name = "Test Coffee", ProcessingMethod = ProcessingMethod.Washed,
            Species = Species.Arabica, RoastLevel = 2.5m, IsBlend = false,
            IsActive = true, CreatedAt = DateTime.UtcNow,
        });
        ctx.Grinders.Add(new Grinder
        {
            Id = 1, Brand = "Baratza", Model = "Encore",
            GrindType = GrindType.Stepped, BurrType = BurrType.Conical,
            ScaleType = ScaleType.Numeric, ScaleMin = 1, ScaleMax = 40, IsVerified = true,
        });
        ctx.GrinderCalibrations.Add(new GrinderCalibration
        {
            GrinderId = 1, BrewMethod = BrewMethod.PourOver,
            NativeSetting = 20m, NgiValue = 50m,
        });
        ctx.SaveChanges();
    }

    [Fact]
    public void Create_ValidRequest_ReturnsOk()
    {
        using var ctx = CreateDb(nameof(Create_ValidRequest_ReturnsOk));
        SeedCoffeeAndGrinder(ctx);
        var ctrl = CreateController(ctx);

        var req = new AddGrindLogRequest(1, 1, BrewMethod.PourOver, 20m, 15m, 250m, 210, 4, null, null, null);
        var result = ctrl.Create(req);

        Assert.IsType<OkObjectResult>(result);
        Assert.Single(ctx.GrindLogs);
    }

    [Fact]
    public void Create_DuplicateRecipeToday_ReturnsConflict()
    {
        using var ctx = CreateDb(nameof(Create_DuplicateRecipeToday_ReturnsConflict));
        SeedCoffeeAndGrinder(ctx);
        var ctrl = CreateController(ctx);

        var req = new AddGrindLogRequest(1, 1, BrewMethod.PourOver, 20m, 15m, 250m, 210, 4, null, null, RecipeId: 5);
        ctrl.Create(req); // first save

        var result = ctrl.Create(req); // duplicate

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Single(ctx.GrindLogs); // only one log saved
    }

    [Fact]
    public void Create_DuplicateWithNoRecipeId_AllowsMultiple()
    {
        // Duplicate detection only applies when RecipeId is set
        using var ctx = CreateDb(nameof(Create_DuplicateWithNoRecipeId_AllowsMultiple));
        SeedCoffeeAndGrinder(ctx);
        var ctrl = CreateController(ctx);

        var req = new AddGrindLogRequest(1, 1, BrewMethod.PourOver, 20m, 15m, 250m, 210, 4, null, null, null);
        ctrl.Create(req);
        ctrl.Create(req); // no recipeId → no duplicate check

        Assert.Equal(2, ctx.GrindLogs.Count());
    }

    [Fact]
    public void Create_WithExtractionFeedback_SavesField()
    {
        using var ctx = CreateDb(nameof(Create_WithExtractionFeedback_SavesField));
        SeedCoffeeAndGrinder(ctx);
        var ctrl = CreateController(ctx);

        var req = new AddGrindLogRequest(1, 1, BrewMethod.PourOver, 20m, 15m, 250m, 210, 4, ExtractionFeedback: 2, null, null);
        ctrl.Create(req);

        Assert.Equal(2, ctx.GrindLogs.First().ExtractionFeedback);
    }

    [Fact]
    public void Create_ExtractionFeedbackOutOfRange_ReturnsBadRequest()
    {
        using var ctx = CreateDb(nameof(Create_ExtractionFeedbackOutOfRange_ReturnsBadRequest));
        SeedCoffeeAndGrinder(ctx);
        var ctrl = CreateController(ctx);

        var req = new AddGrindLogRequest(1, 1, BrewMethod.PourOver, 20m, 15m, 250m, 210, 4, ExtractionFeedback: 5, null, null);
        var result = ctrl.Create(req);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(ctx.GrindLogs);
    }

    [Fact]
    public void Create_SameRecipeDifferentUser_AllowsBoth()
    {
        using var ctx = CreateDb(nameof(Create_SameRecipeDifferentUser_AllowsBoth));
        SeedCoffeeAndGrinder(ctx);
        var ctrl1 = CreateController(ctx, "user-1");
        var ctrl2 = CreateController(ctx, "user-2");

        var req = new AddGrindLogRequest(1, 1, BrewMethod.PourOver, 20m, 15m, 250m, 210, 4, null, null, RecipeId: 1);
        ctrl1.Create(req);
        ctrl2.Create(req);

        Assert.Equal(2, ctx.GrindLogs.Count());
    }

    [Fact]
    public void GetAll_ReturnsOnlyCurrentUserLogs()
    {
        using var ctx = CreateDb(nameof(GetAll_ReturnsOnlyCurrentUserLogs));
        SeedCoffeeAndGrinder(ctx);
        var ctrl1 = CreateController(ctx, "user-1");
        var ctrl2 = CreateController(ctx, "user-2");

        var req = new AddGrindLogRequest(1, 1, BrewMethod.PourOver, 20m, null, null, null, null, null, null, null);
        ctrl1.Create(req);
        ctrl1.Create(req);
        ctrl2.Create(req);

        var result = ctrl1.GetAll(null, null) as OkObjectResult;
        var logs = result!.Value as IEnumerable<GrindLog>;

        Assert.Equal(2, logs!.Count());
    }
}

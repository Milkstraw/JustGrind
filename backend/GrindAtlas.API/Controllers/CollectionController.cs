using GrindAtlas.API.Data;
using GrindAtlas.API.DTOs;
using GrindAtlas.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GrindAtlas.API.Controllers;

// ── Collection: My Shelf + My Setup + Bag Tracking ───────────────────────────
[ApiController, Route("api/collection")]
[Authorize]
public class CollectionController(AppDbContext ctx) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // ── My Shelf (coffees) ────────────────────────────────────────────────────

    [HttpGet("coffees")]
    public IActionResult GetShelf()
    {
        var items = ctx.UserCoffees
            .Where(uc => uc.UserId == UserId)
            .Include(uc => uc.Coffee)
            .OrderBy(uc => uc.Coffee.Name)
            .ToList();
        return Ok(items);
    }

    [HttpPost("coffees/{coffeeId:int}")]
    public IActionResult AddToShelf(int coffeeId)
    {
        if (!ctx.Coffees.Any(c => c.Id == coffeeId))
            return NotFound("Coffee not found.");

        var exists = ctx.UserCoffees.Any(uc => uc.UserId == UserId && uc.CoffeeId == coffeeId);
        if (exists) return Ok(); // idempotent

        ctx.UserCoffees.Add(new UserCoffee { UserId = UserId, CoffeeId = coffeeId });
        ctx.SaveChanges();
        return Ok();
    }

    [HttpDelete("coffees/{coffeeId:int}")]
    public IActionResult RemoveFromShelf(int coffeeId)
    {
        var entry = ctx.UserCoffees.FirstOrDefault(uc => uc.UserId == UserId && uc.CoffeeId == coffeeId);
        if (entry is null) return NoContent();
        ctx.UserCoffees.Remove(entry);
        ctx.SaveChanges();
        return NoContent();
    }

    // ── My Setup (grinders) ───────────────────────────────────────────────────

    [HttpGet("grinders")]
    public IActionResult GetSetupGrinders()
    {
        var items = ctx.UserGrinders
            .Where(ug => ug.UserId == UserId)
            .Include(ug => ug.Grinder).ThenInclude(g => g.Calibrations)
            .OrderBy(ug => ug.Grinder.Brand)
            .ToList();
        return Ok(items);
    }

    [HttpPost("grinders/{grinderId:int}")]
    public IActionResult AddGrinderToSetup(int grinderId)
    {
        if (!ctx.Grinders.Any(g => g.Id == grinderId))
            return NotFound("Grinder not found.");

        var exists = ctx.UserGrinders.Any(ug => ug.UserId == UserId && ug.GrinderId == grinderId);
        if (exists) return Ok();

        ctx.UserGrinders.Add(new UserGrinder { UserId = UserId, GrinderId = grinderId });
        ctx.SaveChanges();
        return Ok();
    }

    [HttpDelete("grinders/{grinderId:int}")]
    public IActionResult RemoveGrinderFromSetup(int grinderId)
    {
        var entry = ctx.UserGrinders.FirstOrDefault(ug => ug.UserId == UserId && ug.GrinderId == grinderId);
        if (entry is null) return NoContent();
        ctx.UserGrinders.Remove(entry);
        ctx.SaveChanges();
        return NoContent();
    }

    // ── My Setup (brew methods) ───────────────────────────────────────────────

    [HttpGet("brewmethods")]
    public IActionResult GetSetupBrewMethods()
    {
        var items = ctx.UserBrewMethods
            .Where(ubm => ubm.UserId == UserId)
            .OrderBy(ubm => ubm.BrewMethod)
            .ToList();
        return Ok(items);
    }

    [HttpPost("brewmethods/{method}")]
    public IActionResult AddBrewMethodToSetup(BrewMethod method)
    {
        var exists = ctx.UserBrewMethods.Any(ubm => ubm.UserId == UserId && ubm.BrewMethod == method);
        if (exists) return Ok();

        ctx.UserBrewMethods.Add(new UserBrewMethod { UserId = UserId, BrewMethod = method });
        ctx.SaveChanges();
        return Ok();
    }

    [HttpDelete("brewmethods/{method}")]
    public IActionResult RemoveBrewMethodFromSetup(BrewMethod method)
    {
        var entry = ctx.UserBrewMethods.FirstOrDefault(ubm => ubm.UserId == UserId && ubm.BrewMethod == method);
        if (entry is null) return NoContent();
        ctx.UserBrewMethods.Remove(entry);
        ctx.SaveChanges();
        return NoContent();
    }

    // ── Bag Tracking ──────────────────────────────────────────────────────────

    [HttpGet("bags")]
    public IActionResult GetBags()
    {
        var bags = ctx.CoffeeBags
            .Where(b => b.UserId == UserId)
            .Include(b => b.Coffee)
            .OrderByDescending(b => b.OpenedAt)
            .ToList();
        return Ok(bags);
    }

    [HttpPost("bags")]
    public IActionResult OpenBag(OpenCoffeeBagRequest req)
    {
        if (!ctx.Coffees.Any(c => c.Id == req.CoffeeId))
            return NotFound("Coffee not found.");

        var bag = new CoffeeBag
        {
            UserId    = UserId,
            CoffeeId  = req.CoffeeId,
            OpenedAt  = DateTime.UtcNow,
            RoastedOn = req.RoastedOn,
            BagWeightG = req.BagWeightG,
            Notes      = req.Notes?.Trim(),
        };
        ctx.CoffeeBags.Add(bag);
        ctx.SaveChanges();
        return Ok(bag);
    }

    [HttpDelete("bags/{bagId:int}")]
    public IActionResult CloseBag(int bagId)
    {
        var bag = ctx.CoffeeBags.FirstOrDefault(b => b.Id == bagId && b.UserId == UserId);
        if (bag is null) return NotFound();
        ctx.CoffeeBags.Remove(bag);
        ctx.SaveChanges();
        return NoContent();
    }

    [HttpGet("bags/{bagId:int}/freshness")]
    public IActionResult GetFreshness(int bagId)
    {
        var bag = ctx.CoffeeBags
            .Include(b => b.Coffee)
            .FirstOrDefault(b => b.Id == bagId && b.UserId == UserId);
        if (bag is null) return NotFound();

        // Compute freshness window based on processing method
        var (peakStart, peakEnd) = GetFreshnessWindow(bag.Coffee.ProcessingMethod);

        int? daysSinceRoast = null;
        string freshnessStatus = "Unknown";

        if (bag.RoastedOn.HasValue)
        {
            daysSinceRoast = (DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - bag.RoastedOn.Value.DayNumber);
            freshnessStatus = daysSinceRoast < peakStart ? "Too Fresh"
                : daysSinceRoast <= peakEnd  ? "Peak"
                : daysSinceRoast <= peakEnd + 14 ? "Acceptable"
                : "Past Peak";
        }

        // Usage rate: total dose from grind logs since bag opened
        var logsAfterOpen = ctx.GrindLogs
            .Where(l => l.UserId == UserId && l.CoffeeId == bag.CoffeeId
                        && l.CreatedAt >= bag.OpenedAt)
            .ToList();

        decimal totalDoseG = logsAfterOpen.Sum(l => l.DoseG ?? 0);
        double daysSinceOpen = Math.Max(1, (DateTime.UtcNow - bag.OpenedAt).TotalDays);
        decimal? usageRate = totalDoseG > 0 ? Math.Round((decimal)(totalDoseG / (decimal)daysSinceOpen), 1) : null;

        decimal? daysRemaining = null;
        bool isRunningLow = false;
        if (bag.BagWeightG.HasValue && usageRate.HasValue && usageRate > 0)
        {
            var remaining = bag.BagWeightG.Value - totalDoseG;
            daysRemaining = Math.Max(0, Math.Round(remaining / usageRate.Value, 0));
            isRunningLow = daysRemaining <= 7;
        }

        return Ok(new FreshnessInfo(
            bag.Id,
            bag.CoffeeId,
            bag.Coffee.Name,
            bag.OpenedAt,
            bag.RoastedOn,
            daysSinceRoast,
            freshnessStatus,
            peakStart,
            peakEnd,
            usageRate,
            daysRemaining,
            isRunningLow
        ));
    }

    private static (int PeakStart, int PeakEnd) GetFreshnessWindow(ProcessingMethod method) => method switch
    {
        ProcessingMethod.Natural   => (14, 35),
        ProcessingMethod.Honey     => (10, 28),
        ProcessingMethod.Anaerobic => (14, 42),
        ProcessingMethod.WetHulled => (7, 21),
        _                          => (7, 21),  // Washed and Other
    };
}

using GrindAtlas.API.Data;
using GrindAtlas.API.DTOs;
using GrindAtlas.API.Models;
using GrindAtlas.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// ── Coffees ──────────────────────────────────────────────────────────────────
namespace GrindAtlas.API.Controllers;

[ApiController, Route("api/[controller]")]
public class CoffeesController(AppDbContext ctx) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll(
        [FromQuery] string? search,
        [FromQuery] ProcessingMethod? processing,
        [FromQuery] decimal? minRoast, [FromQuery] decimal? maxRoast,
        [FromQuery] string? origin)
    {
        var q = ctx.Coffees.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(c => c.Name.Contains(search) || (c.Roaster != null && c.Roaster.Contains(search)));
        if (processing.HasValue)    q = q.Where(c => c.ProcessingMethod == processing.Value);
        if (minRoast.HasValue)      q = q.Where(c => c.RoastLevel >= minRoast.Value);
        if (maxRoast.HasValue)      q = q.Where(c => c.RoastLevel <= maxRoast.Value);
        if (!string.IsNullOrWhiteSpace(origin))
            q = q.Where(c => c.OriginCountry != null && c.OriginCountry.Contains(origin));
        return Ok(q.OrderBy(c => c.Name).ToList());
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var coffee = ctx.Coffees
            .Include(c => c.GrindLogs).ThenInclude(l => l.Grinder)
            .FirstOrDefault(c => c.Id == id);
        return coffee is null ? NotFound() : Ok(coffee);
    }

    [HttpPost]
    public IActionResult Create(Coffee coffee)
    {
        coffee.CreatedAt = DateTime.UtcNow;
        ctx.Coffees.Add(coffee);
        ctx.SaveChanges();
        return CreatedAtAction(nameof(Get), new { id = coffee.Id }, coffee);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, Coffee updated)
    {
        var existing = ctx.Coffees.Find(id);
        if (existing is null) return NotFound();
        updated.Id = id;
        ctx.Entry(existing).CurrentValues.SetValues(updated);
        ctx.SaveChanges();
        return Ok(existing);
    }
}

// ── Grinders ─────────────────────────────────────────────────────────────────
[ApiController, Route("api/[controller]")]
public class GrindersController(AppDbContext ctx) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() =>
        Ok(ctx.Grinders.Include(g => g.Calibrations).OrderBy(g => g.Brand).ToList());

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var g = ctx.Grinders
            .Include(g => g.Calibrations)
            .FirstOrDefault(g => g.Id == id);
        return g is null ? NotFound() : Ok(g);
    }

    [HttpGet("{id}/calibrations")]
    public IActionResult GetCalibrations(int id) =>
        Ok(ctx.GrinderCalibrations.Where(c => c.GrinderId == id).ToList());

    [HttpPost]
    public IActionResult Create(Grinder grinder)
    {
        ctx.Grinders.Add(grinder);
        ctx.SaveChanges();
        return CreatedAtAction(nameof(Get), new { id = grinder.Id }, grinder);
    }
}

// ── GrindLogs ────────────────────────────────────────────────────────────────
[ApiController, Route("api/[controller]")]
public class GrindLogsController(AppDbContext ctx, GrindEstimatorService estimator) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll([FromQuery] int? coffeeId, [FromQuery] int? grinderId)
    {
        var q = ctx.GrindLogs
            .Include(l => l.Coffee)
            .Include(l => l.Grinder)
            .AsQueryable();
        if (coffeeId.HasValue)  q = q.Where(l => l.CoffeeId == coffeeId.Value);
        if (grinderId.HasValue) q = q.Where(l => l.GrinderId == grinderId.Value);
        return Ok(q.OrderByDescending(l => l.CreatedAt).ToList());
    }

    [HttpPost]
    public IActionResult Create(AddGrindLogRequest req)
    {
        var ngi = estimator.NativeToNgi(req.NativeSetting, req.GrinderId);
        var log = new GrindLog
        {
            CoffeeId         = req.CoffeeId,
            GrinderId        = req.GrinderId,
            BrewMethod       = req.BrewMethod,
            NativeSetting    = req.NativeSetting,
            NgiNormalized    = ngi,
            DoseG            = req.DoseG,
            YieldG           = req.YieldG,
            ExtractionTimeS  = req.ExtractionTimeS,
            Rating           = req.Rating,
            Notes            = req.Notes,
            BrewDate         = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt        = DateTime.UtcNow,
        };
        ctx.GrindLogs.Add(log);
        ctx.SaveChanges();
        return Ok(log);
    }
}

// ── Estimator ────────────────────────────────────────────────────────────────
[ApiController, Route("api/[controller]")]
public class EstimatorController(AppDbContext ctx, GrindEstimatorService estimator) : ControllerBase
{
    [HttpPost("estimate")]
    public IActionResult Estimate(EstimateRequest req)
    {
        try
        {
            var result = estimator.Estimate(req.CoffeeId, req.TargetGrinderId, req.BrewMethod);
            var coffee  = ctx.Coffees.Find(req.CoffeeId)!;
            var grinder = ctx.Grinders.Find(req.TargetGrinderId)!;

            var response = new EstimateResponse(
                req.CoffeeId,  coffee.Name,
                req.TargetGrinderId, $"{grinder.Brand} {grinder.Model}",
                req.BrewMethod.ToString(),
                result.EstimatedNativeSetting,
                result.EstimatedNgi,
                result.ConfidenceScore,
                result.InferenceLayer,
                EstimateResponse.LayerLabel(result.InferenceLayer),
                result.SourceLogCount,
                result.AvgSimilarityScore,
                result.Explanation
            );

            // Save audit record
            var audit = new GrindEstimate
            {
                CoffeeId               = req.CoffeeId,
                TargetGrinderId        = req.TargetGrinderId,
                BrewMethod             = req.BrewMethod,
                EstimatedNgi           = result.EstimatedNgi,
                EstimatedNativeSetting = result.EstimatedNativeSetting,
                ConfidenceScore        = (decimal)result.ConfidenceScore,
                InferenceLayer         = result.InferenceLayer,
                SourceLogCount         = result.SourceLogCount,
                AvgSimilarityScore     = (decimal)result.AvgSimilarityScore,
                CreatedAt              = DateTime.UtcNow,
            };
            ctx.GrindEstimates.Add(audit);
            ctx.SaveChanges();

            return Ok(response);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("estimate/{estimateId}/confirm")]
    public IActionResult Confirm(int estimateId, ConfirmEstimateRequest req)
    {
        var est = ctx.GrindEstimates.Find(estimateId);
        if (est is null) return NotFound();
        est.UserConfirmedSetting = req.ConfirmedSetting;
        est.AccuracyDelta = est.EstimatedNativeSetting - req.ConfirmedSetting;
        ctx.SaveChanges();
        return Ok(est);
    }

    [HttpGet("similarity")]
    public IActionResult Similarity([FromQuery] int coffeeAId, [FromQuery] int coffeeBId)
    {
        var a = ctx.Coffees.Find(coffeeAId);
        var b = ctx.Coffees.Find(coffeeBId);
        if (a is null || b is null) return NotFound();
        var sim = estimator.ComputeSimilarity(a, b);
        return Ok(new { CoffeeA = a.Name, CoffeeB = b.Name, SimilarityScore = sim });
    }
}

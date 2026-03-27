using GrindAtlas.API.Data;
using GrindAtlas.API.DTOs;
using GrindAtlas.API.Models;
using GrindAtlas.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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

    [HttpPost, AllowAnonymous]
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

    [HttpPost, AllowAnonymous]
    public IActionResult Create(Grinder grinder)
    {
        ctx.Grinders.Add(grinder);
        ctx.SaveChanges();
        return CreatedAtAction(nameof(Get), new { id = grinder.Id }, grinder);
    }
}

// ── GrindLogs ────────────────────────────────────────────────────────────────
[ApiController, Route("api/[controller]")]
[Authorize]
public class GrindLogsController(AppDbContext ctx, GrindEstimatorService estimator) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll([FromQuery] int? coffeeId, [FromQuery] int? grinderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var q = ctx.GrindLogs
            .Include(l => l.Coffee)
            .Include(l => l.Grinder)
            .Include(l => l.Recipe)
            .Where(l => l.UserId == userId)
            .AsQueryable();
        if (coffeeId.HasValue)  q = q.Where(l => l.CoffeeId == coffeeId.Value);
        if (grinderId.HasValue) q = q.Where(l => l.GrinderId == grinderId.Value);
        return Ok(q.OrderByDescending(l => l.CreatedAt).ToList());
    }

    [HttpPost]
    public IActionResult Create(AddGrindLogRequest req)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Duplicate detection: same recipe + user + today
        if (req.RecipeId.HasValue)
        {
            var duplicate = ctx.GrindLogs.Any(l =>
                l.UserId == userId &&
                l.RecipeId == req.RecipeId &&
                l.BrewDate == today);
            if (duplicate)
                return Conflict("A session for this recipe was already saved today. Remove the existing log first if you want to re-save.");
        }

        if (req.ExtractionFeedback.HasValue && (req.ExtractionFeedback < -3 || req.ExtractionFeedback > 3))
            return BadRequest("ExtractionFeedback must be between -3 and 3.");

        var notes = string.IsNullOrWhiteSpace(req.Notes) ? null : req.Notes.Trim();
        var ngi = estimator.NativeToNgi(req.NativeSetting, req.GrinderId);
        var log = new GrindLog
        {
            CoffeeId           = req.CoffeeId,
            GrinderId          = req.GrinderId,
            BrewMethod         = req.BrewMethod,
            NativeSetting      = req.NativeSetting,
            NgiNormalized      = ngi,
            DoseG              = req.DoseG,
            YieldG             = req.YieldG,
            ExtractionTimeS    = req.ExtractionTimeS,
            Rating             = req.Rating,
            ExtractionFeedback = req.ExtractionFeedback,
            Notes              = notes,
            RecipeId           = req.RecipeId,
            BrewDate           = today,
            CreatedAt          = DateTime.UtcNow,
            UserId             = userId,
        };
        ctx.GrindLogs.Add(log);
        ctx.SaveChanges();
        return Ok(log);
    }
}

// ── Recipes ───────────────────────────────────────────────────────────────────
[ApiController, Route("api/[controller]")]
public class RecipesController(AppDbContext ctx) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() =>
        Ok(ctx.BrewRecipes
            .Include(r => r.Coffee)
            .Include(r => r.Grinder)
            .Include(r => r.Steps.OrderBy(s => s.StepOrder))
            .OrderByDescending(r => r.CreatedAt)
            .ToList());

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var recipe = ctx.BrewRecipes
            .Include(r => r.Coffee)
            .Include(r => r.Grinder)
            .Include(r => r.Steps.OrderBy(s => s.StepOrder))
            .FirstOrDefault(r => r.Id == id);
        return recipe is null ? NotFound() : Ok(recipe);
    }

    [HttpPost]
    public IActionResult Create(CreateBrewRecipeRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest("Recipe name is required.");
        if (req.Steps == null || req.Steps.Count == 0)
            return BadRequest("A recipe must have at least one step.");

        var recipe = new BrewRecipe
        {
            Name           = req.Name.Trim(),
            CoffeeId       = req.CoffeeId,
            GrinderId      = req.GrinderId,
            BrewMethod     = req.BrewMethod,
            NativeSetting  = req.NativeSetting,
            DoseG          = req.DoseG,
            WaterG         = req.WaterG,
            WaterTempC     = req.WaterTempC,
            TechniqueNotes = req.TechniqueNotes?.Trim(),
            CreatedAt      = DateTime.UtcNow,
            Steps          = req.Steps.Select(s => new BrewRecipeStep
            {
                StepOrder   = s.StepOrder,
                Instruction = s.Instruction,
                DurationS   = s.DurationS,
                PourWaterG  = s.PourWaterG,
            }).ToList(),
        };
        ctx.BrewRecipes.Add(recipe);
        ctx.SaveChanges();
        return CreatedAtAction(nameof(Get), new { id = recipe.Id }, recipe);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, CreateBrewRecipeRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest("Recipe name is required.");
        if (req.Steps == null || req.Steps.Count == 0)
            return BadRequest("A recipe must have at least one step.");

        var recipe = ctx.BrewRecipes
            .Include(r => r.Steps)
            .FirstOrDefault(r => r.Id == id);
        if (recipe is null) return NotFound();

        recipe.Name           = req.Name.Trim();
        recipe.CoffeeId       = req.CoffeeId;
        recipe.GrinderId      = req.GrinderId;
        recipe.BrewMethod     = req.BrewMethod;
        recipe.NativeSetting  = req.NativeSetting;
        recipe.DoseG          = req.DoseG;
        recipe.WaterG         = req.WaterG;
        recipe.WaterTempC     = req.WaterTempC;
        recipe.TechniqueNotes = req.TechniqueNotes?.Trim();

        ctx.BrewRecipeSteps.RemoveRange(recipe.Steps);
        recipe.Steps = req.Steps.Select(s => new BrewRecipeStep
        {
            RecipeId    = id,
            StepOrder   = s.StepOrder,
            Instruction = s.Instruction,
            DurationS   = s.DurationS,
            PourWaterG  = s.PourWaterG,
        }).ToList();

        ctx.SaveChanges();
        return Ok(recipe);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var recipe = ctx.BrewRecipes
            .Include(r => r.Steps)
            .FirstOrDefault(r => r.Id == id);
        if (recipe is null) return NotFound();
        ctx.BrewRecipeSteps.RemoveRange(recipe.Steps);
        ctx.BrewRecipes.Remove(recipe);
        ctx.SaveChanges();
        return NoContent();
    }
}

// ── Grind Advisor ─────────────────────────────────────────────────────────────
[ApiController, Route("api/grind-advisor")]
public class GrindAdvisorController(AppDbContext ctx, GrindEstimatorService estimator) : ControllerBase
{
    [HttpPost("estimate")]
    [Authorize]
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
                UserId                 = User.FindFirstValue(ClaimTypes.NameIdentifier),
            };
            ctx.GrindEstimates.Add(audit);
            ctx.SaveChanges();

            return Ok(response);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("estimate/{estimateId}/confirm")]
    [Authorize]
    public IActionResult Confirm(int estimateId, ConfirmEstimateRequest req)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var est = ctx.GrindEstimates.FirstOrDefault(e => e.Id == estimateId && e.UserId == userId);
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

// ── Contact (IT Support) ──────────────────────────────────────────────────────
[ApiController, Route("api/[controller]")]
[AllowAnonymous]
public class ContactController(IEmailService emailService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Send(ContactRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)    ||
            string.IsNullOrWhiteSpace(req.Email)   ||
            string.IsNullOrWhiteSpace(req.Subject) ||
            string.IsNullOrWhiteSpace(req.Message))
            return BadRequest("Name, email, subject, and message are required.");

        if (!new EmailAddressAttribute().IsValid(req.Email))
            return BadRequest("Invalid email address.");

        await emailService.SendSupportNotificationAsync(req.Email, req.Name, req.Subject, req.Message);
        return Ok();
    }
}

// ── Newsletter ────────────────────────────────────────────────────────────────
[ApiController, Route("api/[controller]")]
[Authorize]
public class NewsletterController(IEmailService emailService) : ControllerBase
{
    /// <summary>
    /// Send a newsletter to the provided list of recipient emails.
    /// Requires authentication. Protect this endpoint with an admin role when roles are added.
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> Send(NewsletterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Subject) || string.IsNullOrWhiteSpace(req.HtmlBody))
            return BadRequest("Subject and HtmlBody are required.");

        if (req.Recipients is null || req.Recipients.Count == 0)
            return BadRequest("At least one recipient is required.");

        var valid = req.Recipients
            .Where(r => new EmailAddressAttribute().IsValid(r))
            .ToList();

        await emailService.SendNewsletterAsync(valid, req.Subject, req.HtmlBody);
        return Ok(new { Sent = valid.Count, Skipped = req.Recipients.Count - valid.Count });
    }
}

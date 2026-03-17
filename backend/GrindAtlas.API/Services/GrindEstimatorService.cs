using GrindAtlas.API.Data;
using GrindAtlas.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GrindAtlas.API.Services;

public record EstimateResult(
    decimal EstimatedNativeSetting,
    decimal EstimatedNgi,
    double ConfidenceScore,
    int InferenceLayer,
    int SourceLogCount,
    double AvgSimilarityScore,
    string Explanation
);

public class GrindEstimatorService(AppDbContext ctx)
{
    // ── Similarity weight constants ───────────────────────────────────────────
    private const double W_ROAST     = 0.35;
    private const double W_PROCESS   = 0.20;
    private const double W_ORIGIN    = 0.15;
    private const double W_ELEVATION = 0.15;
    private const double W_VARIETY   = 0.10;
    private const double W_FRESHNESS = 0.05;

    // Processing method groups: similar methods get partial credit
    private static readonly HashSet<ProcessingMethod>[] ProcessGroups =
    [
        [ProcessingMethod.Washed],
        [ProcessingMethod.Natural, ProcessingMethod.Anaerobic],
        [ProcessingMethod.Honey, ProcessingMethod.WetHulled],
    ];

    // Origin region groups: related growing regions
    private static readonly HashSet<string>[] OriginGroups =
    [
        ["Ethiopia", "Kenya", "Rwanda", "Burundi", "Tanzania", "Zimbabwe"],   // East Africa
        ["Colombia", "Peru", "Bolivia", "Ecuador"],                            // Andean
        ["Guatemala", "Costa Rica", "Honduras", "El Salvador", "Nicaragua", "Mexico", "Panama"], // Central America
        ["Brazil", "Dominican Republic"],                                      // Americas lowland
        ["Indonesia", "Papua New Guinea", "Vietnam", "India"],                 // Asia/Pacific
        ["Yemen"],                                                             // Arabian Peninsula
        ["Jamaica", "USA"],                                                    // Other
        ["China"],
    ];

    public EstimateResult Estimate(int coffeeId, int targetGrinderId, BrewMethod brewMethod)
    {
        var targetCoffee = ctx.Coffees.Find(coffeeId)
            ?? throw new ArgumentException($"Coffee {coffeeId} not found");

        // ── Layer 1: Exact coffee + exact grinder + exact brew method ─────────
        var directLogs = ctx.GrindLogs
            .Where(l => l.CoffeeId == coffeeId &&
                        l.GrinderId == targetGrinderId &&
                        l.BrewMethod == brewMethod)
            .ToList();

        if (directLogs.Count != 0)
        {
            var avgNgi = (decimal)directLogs.Average(l => (double)l.NgiNormalized);
            var native = NgiToNative(avgNgi, targetGrinderId, brewMethod);
            return new EstimateResult(
                native, avgNgi, 0.95, 1, directLogs.Count, 1.0,
                $"Direct match: {directLogs.Count} previous log(s) for this exact coffee on this grinder."
            );
        }

        // ── Layer 2: Same coffee, any grinder → bridge via NGI ───────────────
        var crossGrinderLogs = ctx.GrindLogs
            .Where(l => l.CoffeeId == coffeeId && l.BrewMethod == brewMethod)
            .ToList();

        if (crossGrinderLogs.Count != 0)
        {
            var avgNgi = (decimal)crossGrinderLogs.Average(l => (double)l.NgiNormalized);
            var native = NgiToNative(avgNgi, targetGrinderId, brewMethod);
            return new EstimateResult(
                native, avgNgi, 0.82, 2, crossGrinderLogs.Count, 1.0,
                $"Cross-grinder transfer: this coffee was logged on {crossGrinderLogs.Select(l => l.GrinderId).Distinct().Count()} other grinder(s)."
            );
        }

        // ── Compute similarity scores for all other coffees ──────────────────
        var allCoffees = ctx.Coffees.Where(c => c.Id != coffeeId).ToList();
        var scored = allCoffees
            .Select(c => (Coffee: c, Sim: ComputeSimilarity(targetCoffee, c)))
            .Where(x => x.Sim >= 0.40)
            .OrderByDescending(x => x.Sim)
            .Take(15)
            .ToList();

        if (scored.Count == 0)
            return FallbackEstimate(targetGrinderId, brewMethod);

        var similarIds = scored.Select(x => x.Coffee.Id).ToHashSet();

        // ── Layer 3: Similar coffees on the same target grinder ───────────────
        var layer3Logs = ctx.GrindLogs
            .Where(l => similarIds.Contains(l.CoffeeId) &&
                        l.GrinderId == targetGrinderId &&
                        l.BrewMethod == brewMethod)
            .ToList();

        if (layer3Logs.Count != 0)
        {
            var (weightedNgi, avgSim) = WeightedAvgNgi(layer3Logs, scored);
            var native = NgiToNative(weightedNgi, targetGrinderId, brewMethod);
            var confidence = 0.50 + avgSim * 0.30;
            return new EstimateResult(
                native, weightedNgi, confidence, 3, layer3Logs.Count, avgSim,
                $"Similar-coffee estimate from {layer3Logs.Count} log(s) on this grinder (avg similarity {avgSim:P0})."
            );
        }

        // ── Layer 4: Similar coffees on ANY grinder ──────────────────────────
        var layer4Logs = ctx.GrindLogs
            .Where(l => similarIds.Contains(l.CoffeeId) && l.BrewMethod == brewMethod)
            .ToList();

        if (layer4Logs.Count != 0)
        {
            var (weightedNgi, avgSim) = WeightedAvgNgi(layer4Logs, scored);
            var native = NgiToNative(weightedNgi, targetGrinderId, brewMethod);
            var confidence = 0.30 + avgSim * 0.25;
            return new EstimateResult(
                native, weightedNgi, confidence, 4, layer4Logs.Count, avgSim,
                $"Broad estimate from {layer4Logs.Count} similar-coffee log(s) across all grinders (avg similarity {avgSim:P0})."
            );
        }

        return FallbackEstimate(targetGrinderId, brewMethod);
    }

    // ── Coffee similarity scoring (0–1) ──────────────────────────────────────
    public double ComputeSimilarity(Coffee a, Coffee b)
    {
        double score = 0;

        // Roast level (35%) — max diff = 4
        score += W_ROAST * (1.0 - Math.Abs((double)(a.RoastLevel - b.RoastLevel)) / 4.0);

        // Processing method (20%)
        double procScore = a.ProcessingMethod == b.ProcessingMethod ? 1.0
            : ProcessGroups.Any(g => g.Contains(a.ProcessingMethod) && g.Contains(b.ProcessingMethod)) ? 0.5
            : 0.0;
        score += W_PROCESS * procScore;

        // Origin (15%)
        double originScore = a.OriginCountry == b.OriginCountry ? 1.0
            : OriginGroups.Any(g => g.Contains(a.OriginCountry ?? "") && g.Contains(b.OriginCountry ?? "")) ? 0.5
            : 0.0;
        score += W_ORIGIN * originScore;

        // Elevation (15%)
        if (a.ElevationMasl.HasValue && b.ElevationMasl.HasValue)
            score += W_ELEVATION * (1.0 - Math.Min(Math.Abs(a.ElevationMasl.Value - b.ElevationMasl.Value) / 1500.0, 1.0));
        else
            score += W_ELEVATION * 0.5; // neutral when unknown

        // Variety (10%)
        score += W_VARIETY * (a.Variety == b.Variety ? 1.0 : 0.0);

        // Freshness (5%) — neutral for seed data (no roast date difference)
        score += W_FRESHNESS * 0.5;

        return Math.Round(score, 4);
    }

    // ── NGI <→ Native setting conversion ─────────────────────────────────────
    public decimal NgiToNative(decimal ngi, int grinderId, BrewMethod brewMethod)
    {
        var anchors = ctx.GrinderCalibrations
            .Where(c => c.GrinderId == grinderId)
            .OrderBy(c => c.NgiValue)
            .ToList();

        if (anchors.Count == 0)
            throw new InvalidOperationException($"No calibrations for grinder {grinderId}");

        // Clamp to range
        if (ngi <= anchors.First().NgiValue) return anchors.First().NativeSetting;
        if (ngi >= anchors.Last().NgiValue)  return anchors.Last().NativeSetting;

        // Linear interpolation between nearest anchors
        for (int i = 0; i < anchors.Count - 1; i++)
        {
            var lo = anchors[i];
            var hi = anchors[i + 1];
            if (ngi >= lo.NgiValue && ngi <= hi.NgiValue)
            {
                var t = (ngi - lo.NgiValue) / (hi.NgiValue - lo.NgiValue);
                var result = lo.NativeSetting + t * (hi.NativeSetting - lo.NativeSetting);
                return Math.Round(result, 1);
            }
        }

        return anchors.Last().NativeSetting;
    }

    public decimal NativeToNgi(decimal nativeSetting, int grinderId)
    {
        var anchors = ctx.GrinderCalibrations
            .Where(c => c.GrinderId == grinderId)
            .OrderBy(c => c.NativeSetting)
            .ToList();

        if (anchors.Count == 0) return 50m;
        if (nativeSetting <= anchors.First().NativeSetting) return anchors.First().NgiValue;
        if (nativeSetting >= anchors.Last().NativeSetting)  return anchors.Last().NgiValue;

        for (int i = 0; i < anchors.Count - 1; i++)
        {
            var lo = anchors[i];
            var hi = anchors[i + 1];
            if (nativeSetting >= lo.NativeSetting && nativeSetting <= hi.NativeSetting)
            {
                var t = (nativeSetting - lo.NativeSetting) / (hi.NativeSetting - lo.NativeSetting);
                return Math.Round(lo.NgiValue + t * (hi.NgiValue - lo.NgiValue), 2);
            }
        }
        return anchors.Last().NgiValue;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static (decimal weightedNgi, double avgSim) WeightedAvgNgi(
        List<GrindLog> logs,
        List<(Coffee Coffee, double Sim)> scored)
    {
        var simMap = scored.ToDictionary(x => x.Coffee.Id, x => x.Sim);
        double totalWeight = 0, weightedSum = 0;

        foreach (var log in logs)
        {
            var w = simMap.TryGetValue(log.CoffeeId, out var s) ? s : 0.0;
            weightedSum += (double)log.NgiNormalized * w;
            totalWeight  += w;
        }

        var avgNgi  = totalWeight > 0 ? (decimal)(weightedSum / totalWeight) : logs.Average(l => l.NgiNormalized);
        var avgSim  = totalWeight > 0 ? weightedSum / totalWeight / (double)avgNgi : 0.5;
        // Fix: compute avgSim properly
        var avgSimFix = logs.Count > 0
            ? logs.Average(l => simMap.TryGetValue(l.CoffeeId, out var s2) ? s2 : 0.0)
            : 0.0;

        return (Math.Round(avgNgi, 2), Math.Round(avgSimFix, 4));
    }

    private EstimateResult FallbackEstimate(int grinderId, BrewMethod brewMethod)
    {
        // Return the brew-method anchor NGI as the fallback
        var anchor = ctx.GrinderCalibrations
            .Where(c => c.GrinderId == grinderId && c.BrewMethod == brewMethod)
            .FirstOrDefault();

        if (anchor != null)
            return new EstimateResult(
                anchor.NativeSetting, anchor.NgiValue, 0.25, 4, 0, 0,
                "No similar coffees found — returning brew method baseline setting."
            );

        // Generic midpoint fallback
        var grinder = ctx.Grinders.Find(grinderId);
        if (grinder == null) throw new ArgumentException($"Grinder {grinderId} not found");
        var midpoint = (grinder.ScaleMin + grinder.ScaleMax) / 2;
        return new EstimateResult(midpoint, 50m, 0.10, 4, 0, 0,
            "No data available — returning midpoint setting.");
    }
}

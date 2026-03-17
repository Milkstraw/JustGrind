namespace GrindAtlas.API.Models;

public class GrindEstimate
{
    public int Id { get; set; }
    public int CoffeeId { get; set; }
    public int TargetGrinderId { get; set; }
    public BrewMethod BrewMethod { get; set; }
    public decimal EstimatedNgi { get; set; }
    public decimal EstimatedNativeSetting { get; set; }
    public decimal ConfidenceScore { get; set; }   // 0–1
    public int InferenceLayer { get; set; }        // 1–4
    public int SourceLogCount { get; set; }
    public decimal AvgSimilarityScore { get; set; }
    public decimal? UserConfirmedSetting { get; set; }
    public decimal? AccuracyDelta { get; set; }    // estimated - confirmed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Coffee Coffee { get; set; } = null!;
    public Grinder TargetGrinder { get; set; } = null!;
}

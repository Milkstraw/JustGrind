using GrindAtlas.API.Models;

namespace GrindAtlas.API.DTOs;

public record EstimateRequest(int CoffeeId, int TargetGrinderId, BrewMethod BrewMethod);

public record EstimateResponse(
    int CoffeeId,
    string CoffeeName,
    int GrinderId,
    string GrinderName,
    string BrewMethod,
    decimal EstimatedNativeSetting,
    decimal EstimatedNgi,
    double ConfidenceScore,
    int InferenceLayer,
    string InferenceLayerLabel,
    int SourceLogCount,
    double AvgSimilarityScore,
    string Explanation
)
{
    public static string LayerLabel(int layer) => layer switch
    {
        1 => "Direct Hit",
        2 => "Cross-Grinder Transfer",
        3 => "Similar Coffee / Same Grinder",
        4 => "Similar Coffee / Any Grinder",
        _ => "Fallback Baseline"
    };
}

public record AddGrindLogRequest(
    int CoffeeId,
    int GrinderId,
    BrewMethod BrewMethod,
    decimal NativeSetting,
    decimal? DoseG,
    decimal? YieldG,
    int? ExtractionTimeS,
    int? Rating,
    string? Notes,
    int? RecipeId
);

public record ConfirmEstimateRequest(decimal ConfirmedSetting);

public record BrewRecipeStepRequest(int StepOrder, string Instruction, int DurationS, decimal? PourWaterG);

public record CreateBrewRecipeRequest(
    string Name,
    int CoffeeId,
    int GrinderId,
    BrewMethod BrewMethod,
    decimal? NativeSetting,
    decimal? DoseG,
    decimal? WaterG,
    decimal? WaterTempC,
    string? TechniqueNotes,
    List<BrewRecipeStepRequest> Steps
);

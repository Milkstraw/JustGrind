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
    int? ExtractionFeedback,  // -3 (very under) to +3 (very over)
    string? Notes,
    int? RecipeId
);

public record OpenCoffeeBagRequest(
    int CoffeeId,
    DateOnly? RoastedOn,
    decimal? BagWeightG,
    string? Notes
);

public record FreshnessInfo(
    int BagId,
    int CoffeeId,
    string CoffeeName,
    DateTime OpenedAt,
    DateOnly? RoastedOn,
    int? DaysSinceRoast,
    string FreshnessStatus,   // "Too Fresh" | "Peak" | "Acceptable" | "Past Peak"
    int? PeakStartDay,
    int? PeakEndDay,
    decimal? UsageRateGPerDay,
    decimal? EstimatedDaysRemaining,
    bool IsRunningLow
);

public record ConfirmEstimateRequest(decimal ConfirmedSetting);

public record RegisterRequest(string Email, string Password, string? DisplayName);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Email, string? DisplayName, bool IsAdmin);

public record AdminAuditLogResponse(
    int Id,
    string ActorId,
    string ActorEmail,
    string Action,
    string EntityType,
    string? EntityId,
    string? OldValue,
    string? NewValue,
    string? Notes,
    string? IpAddress,
    DateTime Timestamp
);

public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string UserId, string Token, string NewPassword);

public record ContactRequest(string Name, string Email, string Subject, string Message);
public record NewsletterRequest(List<string> Recipients, string Subject, string HtmlBody);

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

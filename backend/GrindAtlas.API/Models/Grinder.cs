namespace GrindAtlas.API.Models;

public class Grinder
{
    public int Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public GrindType GrindType { get; set; }
    public BurrType BurrType { get; set; }
    public ScaleType ScaleType { get; set; } = ScaleType.Numeric;
    /// <summary>For AlphaNumeric scales: how many numeric sub-positions per letter (e.g. 9 for A1–A9).</summary>
    public int? ScaleSubDivisions { get; set; }
    /// <summary>For AlphaNumeric scales: order of letter and number. "A1" = letter-first (default), "1A" = number-first.</summary>
    public string? ScaleFormat { get; set; }
    /// <summary>For AlphaNumeric scales: whether sub-positions are "Stepped" or "Stepless".</summary>
    public string? ScaleSubType { get; set; }
    public int? BurrSizeMm { get; set; }
    public decimal ScaleMin { get; set; }
    public decimal ScaleMax { get; set; }
    public string? Notes { get; set; }
    public bool IsVerified { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<GrinderCalibration> Calibrations { get; set; } = [];
    public ICollection<GrindLog> GrindLogs { get; set; } = [];
}

public class GrinderCalibration
{
    public int Id { get; set; }
    public int GrinderId { get; set; }
    public BrewMethod BrewMethod { get; set; }
    public decimal NativeSetting { get; set; }   // Grinder's own scale
    public decimal NgiValue { get; set; }         // 0–100 Normalized Grind Index
    public string? AnchorLabel { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [System.Text.Json.Serialization.JsonIgnore]
    public Grinder Grinder { get; set; } = null!;
}

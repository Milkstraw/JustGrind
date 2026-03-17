namespace GrindAtlas.API.Models;

public class GrindLog
{
    public int Id { get; set; }
    public int CoffeeId { get; set; }
    public int GrinderId { get; set; }
    public BrewMethod BrewMethod { get; set; }
    public decimal NativeSetting { get; set; }
    public decimal NgiNormalized { get; set; }     // computed on save via interpolation
    public decimal? DoseG { get; set; }
    public decimal? YieldG { get; set; }
    public int? ExtractionTimeS { get; set; }
    public decimal? TdsPercent { get; set; }
    public int? Rating { get; set; }               // 1–5
    public string? Notes { get; set; }
    public DateOnly? BrewDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Coffee Coffee { get; set; } = null!;
    public Grinder Grinder { get; set; } = null!;
}

public class BrewRecipe
{
    public int Id { get; set; }
    public int CoffeeId { get; set; }
    public int GrinderId { get; set; }
    public BrewMethod BrewMethod { get; set; }
    public decimal? NativeSetting { get; set; }
    public decimal? DoseG { get; set; }
    public decimal? WaterG { get; set; }
    public decimal? WaterTempC { get; set; }
    public int? BloomTimeS { get; set; }
    public decimal? BloomWaterG { get; set; }
    public int? TotalTimeS { get; set; }
    public string? TechniqueNotes { get; set; }
    public int? Rating { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Coffee Coffee { get; set; } = null!;
    public Grinder Grinder { get; set; } = null!;
}

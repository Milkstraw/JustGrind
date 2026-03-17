namespace GrindAtlas.API.Models;

public class Coffee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Roaster { get; set; }
    public string? OriginCountry { get; set; }
    public string? OriginRegion { get; set; }
    public int? ElevationMasl { get; set; }
    public ProcessingMethod ProcessingMethod { get; set; }
    public string? Variety { get; set; }
    public Species Species { get; set; } = Species.Arabica;
    public decimal RoastLevel { get; set; }        // 1.0 (light) – 5.0 (dark)
    public DateOnly? RoastDate { get; set; }
    public bool IsBlend { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public string? TastingNotes { get; set; }      // CSV: "blueberry,jasmine,citrus"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Nav
    public ICollection<GrindLog> GrindLogs { get; set; } = [];
    public ICollection<BrewRecipe> BrewRecipes { get; set; } = [];
}

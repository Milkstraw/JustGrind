namespace GrindAtlas.API.Models;

public class UserCoffee
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public int CoffeeId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public Coffee Coffee { get; set; } = null!;
}

public class UserGrinder
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public int GrinderId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public Grinder Grinder { get; set; } = null!;
}

public class UserBrewMethod
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public BrewMethod BrewMethod { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}

public class CoffeeBag
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public int CoffeeId { get; set; }
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateOnly? RoastedOn { get; set; }
    public decimal? BagWeightG { get; set; }
    public string? Notes { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Coffee Coffee { get; set; } = null!;
}

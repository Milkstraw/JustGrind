using Microsoft.AspNetCore.Identity;

namespace GrindAtlas.API.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public ICollection<GrindLog> GrindLogs { get; set; } = [];
}

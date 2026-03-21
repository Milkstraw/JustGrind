using GrindAtlas.API.DTOs;
using GrindAtlas.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GrindAtlas.API.Controllers;

[ApiController, Route("api/[controller]")]
[AllowAnonymous]
public class AuthController(UserManager<ApplicationUser> userManager, IConfiguration config) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        var user = new ApplicationUser
        {
            Email = req.Email,
            UserName = req.Email,
            DisplayName = req.DisplayName,
        };

        var result = await userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok(await GenerateToken(user));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, req.Password))
            return Unauthorized("Invalid email or password.");

        return Ok(await GenerateToken(user));
    }

    private async Task<AuthResponse> GenerateToken(ApplicationUser user)
    {
        var roles   = await userManager.GetRolesAsync(user);
        var isAdmin = roles.Contains("Admin");

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new("displayName", user.DisplayName ?? ""),
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.Email!,
            user.DisplayName,
            isAdmin
        );
    }
}

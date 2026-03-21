using GrindAtlas.API.DTOs;
using GrindAtlas.API.Models;
using GrindAtlas.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GrindAtlas.API.Controllers;

[ApiController, Route("api/[controller]")]
[AllowAnonymous]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    IConfiguration config,
    IEmailService emailService) : ControllerBase
{
    // ── Register ──────────────────────────────────────────────────────────────

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        var user = new ApplicationUser
        {
            Email       = req.Email,
            UserName    = req.Email,
            DisplayName = req.DisplayName,
        };

        var result = await userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        // Send welcome email (fire-and-forget; don't block registration on email failure)
        _ = SendVerificationEmailAsync(user);

        return Ok(GenerateToken(user));
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, req.Password))
            return Unauthorized("Invalid email or password.");

        return Ok(GenerateToken(user));
    }

    // ── Email verification ────────────────────────────────────────────────────

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification(ForgotPasswordRequest req)
    {
        var user = await userManager.FindByEmailAsync(req.Email);
        // Always 200 to avoid user enumeration
        if (user is not null && !await userManager.IsEmailConfirmedAsync(user))
            _ = SendVerificationEmailAsync(user);
        return Ok();
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return BadRequest("Invalid link.");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok("Email confirmed. You can now log in.");
    }

    // ── Password reset ────────────────────────────────────────────────────────

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest req)
    {
        var user = await userManager.FindByEmailAsync(req.Email);
        // Always 200 to avoid user enumeration
        if (user is not null)
        {
            var rawToken  = await userManager.GeneratePasswordResetTokenAsync(user);
            var safeToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
            var frontendUrl = config["EmailSettings:FrontendBaseUrl"] ?? "http://localhost:4200";
            var resetUrl  = $"{frontendUrl}/auth/reset-password?userId={user.Id}&token={safeToken}";
            _ = emailService.SendPasswordResetAsync(user.Email!, user.DisplayName ?? user.Email!, resetUrl);
        }
        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest req)
    {
        var user = await userManager.FindByIdAsync(req.UserId);
        if (user is null) return BadRequest("Invalid request.");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(req.Token));
        var result = await userManager.ResetPasswordAsync(user, decodedToken, req.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok("Password reset successfully.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task SendVerificationEmailAsync(ApplicationUser user)
    {
        var rawToken  = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var safeToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
        var frontendUrl = config["EmailSettings:FrontendBaseUrl"] ?? "http://localhost:4200";
        var confirmUrl  = $"{frontendUrl}/auth/confirm-email?userId={user.Id}&token={safeToken}";

        await emailService.SendWelcomeAsync(user.Email!, user.DisplayName ?? user.Email!);
        await emailService.SendVerificationAsync(user.Email!, user.DisplayName ?? user.Email!, confirmUrl);
    }

    private AuthResponse GenerateToken(ApplicationUser user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim("displayName", user.DisplayName ?? ""),
        };

        var token = new JwtSecurityToken(
            issuer:            config["Jwt:Issuer"],
            audience:          config["Jwt:Audience"],
            claims:            claims,
            expires:           DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.Email!,
            user.DisplayName
        );
    }
}

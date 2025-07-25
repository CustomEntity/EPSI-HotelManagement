using System.Security.Claims;
using HotelManagement.Identity.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IIdentityService identityService, ILogger<AuthController> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _identityService.RegisterAsync(request);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.Value),
            new(ClaimTypes.Email, request.Email),
            new(ClaimTypes.Name, request.FirstName + " " + request.LastName)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        return Ok(new { message = "Registration successful", userId = result.Value });
    }
    
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        Console.WriteLine("Logging out user...");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "Logout successful" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _identityService.LoginAsync(request);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.Value.UserId),
            new(ClaimTypes.Email, result.Value.Email),
            new(ClaimTypes.Name, result.Value.FirstName + " " + result.Value.LastName)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        return Ok(new { message = "Login successful", user = result.Value });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _identityService.ChangePasswordAsync(
                userId,
                request.CurrentPassword,
                request.NewPassword);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, new { error = "An error occurred while changing password" });
        }
    }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
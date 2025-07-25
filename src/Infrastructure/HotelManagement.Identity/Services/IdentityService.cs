using HotelManagement.Domain.Shared;
using HotelManagement.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;

namespace HotelManagement.Identity.Services;

public interface IIdentityService
{
    Task<Result<string>> RegisterAsync(RegisterRequest request);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<Result<ApplicationUser>> GetUserByIdAsync(string userId);
}

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<Result<string>> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Result<string>.Failure("User with this email already exists");

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role ?? "Customer",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<string>.Failure($"Failed to create user: {errors}");
        }

        // Ajouter au rôle
        await _userManager.AddToRoleAsync(user, user.Role);

        return Result<string>.Success(user.Id);
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<LoginResponse>.Failure("Invalid email or password");

        if (!user.IsActive)
            return Result<LoginResponse>.Failure("Account is deactivated");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return Result<LoginResponse>.Failure("Invalid email or password");

        var token = _tokenService.GenerateToken(user);
        
        return Result<LoginResponse>.Success(new LoginResponse
        {
            Token = token,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            UserId = user.Id
        });
    }

    public async Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure("User not found");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure($"Failed to change password: {errors}");
        }

        return Result.Success();
    }

    public async Task<Result<ApplicationUser>> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<ApplicationUser>.Failure("User not found");

        return Result<ApplicationUser>.Success(user);
    }
}

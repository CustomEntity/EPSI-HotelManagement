using System.Security.Claims;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Identity.Domain;
using Microsoft.AspNetCore.Http;

namespace HotelManagement.Infrastructure;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
                
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? Email => 
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public UserRole? Role
    {
        get
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : null;
        }
    }

    public bool IsAuthenticated => 
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool HasPermission(string permission)
    {
        if (!Role.HasValue) return false;
        return RolePermissions.HasPermission(Role.Value, permission);
    }

    public bool IsInRole(UserRole role)
    {
        return Role.HasValue && Role.Value == role;
    }
}
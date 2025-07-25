using HotelManagement.Identity.Domain;

namespace HotelManagement.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
    bool HasPermission(string permission);
    bool IsInRole(UserRole role);
}
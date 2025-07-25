using Microsoft.AspNetCore.Identity;

namespace HotelManagement.Identity.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
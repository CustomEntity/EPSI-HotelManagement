using HotelManagement.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Identity.Data;

public class IdentityContext : IdentityDbContext<ApplicationUser>
{
    public IdentityContext(DbContextOptions<IdentityContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "1", Name = "Customer", NormalizedName = "CUSTOMER" },
            new IdentityRole { Id = "2", Name = "Receptionist", NormalizedName = "RECEPTIONIST" },
            new IdentityRole { Id = "3", Name = "HousekeepingStaff", NormalizedName = "HOUSEKEEPINGSTAFF" },
            new IdentityRole { Id = "4", Name = "Manager", NormalizedName = "MANAGER" },
            new IdentityRole { Id = "5", Name = "Administrator", NormalizedName = "ADMINISTRATOR" }
        );
    }
}
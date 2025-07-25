using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HotelManagement.Identity.Data;
using HotelManagement.Identity.Models;
using HotelManagement.Identity.Services;

namespace HotelManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        
        // Add Identity
        services.AddIdentity(configuration);
        
        return services;
    }

    public static IServiceCollection AddIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext pour Identity
        services.AddDbContext<IdentityContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("IdentityConnection"),
                b => b.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName)));

        // Configuration Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;

            // User settings
            options.User.RequireUniqueEmail = true;
            
            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddEntityFrameworkStores<IdentityContext>()
        .AddDefaultTokenProviders();

        // JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // TODO: Set to true in production
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"))),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}

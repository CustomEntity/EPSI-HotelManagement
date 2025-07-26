using HotelManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HotelManagement.Domain.Booking;
using HotelManagement.Domain.Booking.Services;
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Room;
using HotelManagement.Identity.Data;
using HotelManagement.Identity.Models;
using HotelManagement.Identity.Services;
using HotelManagement.Infrastructure.Features.Booking.Repositories;
using HotelManagement.Infrastructure.Features.Booking.Services;
using HotelManagement.Infrastructure.Features.Repositories;
using HotelManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace HotelManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentity(configuration);
        
        services.AddPersistence(configuration);
        
        return services;
    }
    
    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IBookingRepository, InMemoryBookingRepository>();
        
        services.AddScoped<IRoomRepository, InMemoryRoomRepository>();
        
        services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();
        
        services.AddDomainServices();
        
        return services;
    }

    private static IServiceCollection AddDomainServices(
        this IServiceCollection services)
    {
        services.AddScoped<IBookingDomainService, BookingDomainService>();

        return services;
    }

    public static IServiceCollection AddIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<IdentityContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("IdentityConnection"),
                b => b.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName)));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                options.User.RequireUniqueEmail = true;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.Name = ".AspNetCore.Cookies";

                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(
                            "{\"error\":\"Unauthorized\",\"message\":\"Authentication required\"}");
                    },
                    OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("{\"error\":\"Forbidden\",\"message\":\"Access denied\"}");
                    },
                    OnRedirectToLogout = context =>
                    {
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("{\"message\":\"Logged out\"}");
                    }
                };
            });

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
using HotelManagement.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpContextAccessor AVANT d'ajouter Infrastructure
builder.Services.AddHttpContextAccessor();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/api/auth/login";
        options.LogoutPath = "/api/auth/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true; // Protection XSS
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS uniquement
        options.Cookie.SameSite = SameSiteMode.Strict; // Protection CSRF
    });

// Add application services
//builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add CORS si nécessaire
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication(); // IMPORTANT: Avant UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();
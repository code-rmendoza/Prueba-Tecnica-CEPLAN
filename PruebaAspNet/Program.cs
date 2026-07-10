using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PruebaAspNet.Constants;
using PruebaAspNet.Data;
using PruebaAspNet.Middleware;
using PruebaAspNet.Models;
using PruebaAspNet.Repositories;
using PruebaAspNet.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration Models ─────────────────────────────────────────────
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));

// ── Entity Framework Core ──────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Dependency Injection ───────────────────────────────────────────────
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// ── Rate Limiting ─────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = AppConstants.RateLimitPermitLimit,
                Window = TimeSpan.FromMinutes(AppConstants.RateLimitWindowMinutes)
            }));
});

// ── Cookie Authentication ──────────────────────────────────────────────
var minutosSesion = builder.Configuration.GetValue<int>("Seguridad:MinutosSesion", 30);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(minutosSesion);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

// ── MVC ────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();



var app = builder.Build();

// ── Seed de datos iniciales ────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await DbSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex,
            "No se pudo conectar a la base de datos o sembrar datos. " +
            "Verifique la cadena de conexión en appsettings.json.");
    }
}

// ── Middleware Pipeline ────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// ── Rutas ──────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Activated}/{id?}");

app.Run();

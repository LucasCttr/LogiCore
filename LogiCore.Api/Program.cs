using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LogiCore.Infrastructure.Persistence;
using StackExchange.Redis;
using LogiCore.Infrastructure.Services.Redis;
using LogiCore.Application.Services;
using LogiCore.Application.Common.Interfaces;
using LogiCore.Domain.Entities;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using LogiCore.Application.Features.Packages;
using MediatR;
using LogiCore.Application.Common.Behaviors;
using Serilog;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// --- Logging ---
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// --- Controllers ---
builder.Services.AddControllers(options =>
{
    options.Filters.Add<LogiCore.Api.Filters.ResultActionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(CreatePackageCommandHandler).Assembly);

// --- CORS ---
builder.Services.AddCors(options =>
{
    // Read allowed origins from environment variable ALLOWED_ORIGINS (comma separated)
    var allowed = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
    string[] origins = Array.Empty<string>();
    if (!string.IsNullOrWhiteSpace(allowed))
    {
        origins = allowed.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    options.AddPolicy("AllowFrontend", policy =>
    {
        if (origins.Length == 0)
        {
            // Default to localhost during development
            policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            // If a wildcard '*' is provided, allow any origin (useful for quick testing only)
            if (origins.Length == 1 && origins[0] == "*")
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
            else
            {
                policy.WithOrigins(origins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
        }
    });
});

// --- Persistence ---
string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string? connectionString = null;
if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    // Railway provides DATABASE_URL in the form: postgres://user:pass@host:port/dbname
    var npgsqlBuilder = new Npgsql.NpgsqlConnectionStringBuilder(databaseUrl);
    connectionString = npgsqlBuilder.ToString();
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Database connection string is not configured. Set DATABASE_URL or ConnectionStrings:DefaultConnection.");

builder.Services.AddDbContext<LogiCoreDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IPackageRepository, SqlPackageRepository>();
builder.Services.AddScoped<IShipmentRepository, SqlShipmentRepository>();
builder.Services.AddScoped<IVehicleRepository, SqlVehicleRepository>();
builder.Services.AddScoped<IDriverRepository, SqlDriverRepository>();
builder.Services.AddScoped<ILocationRepository, SqlLocationRepository>();
builder.Services.AddScoped<LogiCore.Application.Repositories.IDriverDetailsRepository, LogiCore.Infrastructure.Repositories.DriverDetailsRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Infrastructure services
builder.Services.AddTransient<LogiCore.Application.Common.Interfaces.INotificationService, LogiCore.Infrastructure.Services.ConsoleNotificationService>();
builder.Services.AddTransient<LogiCore.Application.Common.Interfaces.IEmailService, LogiCore.Infrastructure.Services.SmtpEmailService>();
builder.Services.AddTransient<LogiCore.Domain.Common.Interfaces.ICostCalculator, LogiCore.Infrastructure.Services.StandardCostCalculator>();
builder.Services.AddScoped<LogiCore.Application.Common.Interfaces.IMetricsService, LogiCore.Infrastructure.Services.DatabaseMetricsService>();
builder.Services.AddScoped<LogiCore.Application.Services.IDriverDetailsService, LogiCore.Infrastructure.Services.DriverDetailsService>();
// Background publisher that exports business metrics as Prometheus Gauges
builder.Services.AddHostedService<LogiCore.Infrastructure.Services.PrometheusMetricsPublisher>();

// --- Redis autocomplete service ---
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var conn = builder.Configuration.GetValue<string>("Redis:Connection") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(conn);
});

// Infrastructure implements repository interface defined in Application
builder.Services.AddSingleton<IAddressAutocompleteRepository, RedisAddressAutocompleteService>();
// Application service enforces business rules and is the dependency for controllers
builder.Services.AddScoped<IAddressAutocompleteService, AddressAutocompleteService>();

// --- Identity and Authentication ---
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<LogiCoreDbContext>()
    .AddDefaultTokenProviders();

var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<LogiCore.Application.Common.Models.JwtSettings>(jwtSection);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
    .AddJwtBearer("JwtBearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SecretKey"] ?? "")),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddSingleton<LogiCore.Application.Common.Interfaces.Security.IJwtProvider, LogiCore.Infrastructure.Security.JwtProvider>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<LogiCore.Application.Common.Interfaces.Security.ICurrentUserService, LogiCore.Infrastructure.Security.CurrentUserService>();
// Refresh token service (persists refresh tokens)
builder.Services.AddScoped<LogiCore.Application.Common.Interfaces.Security.IRefreshTokenService, LogiCore.Infrastructure.Security.RefreshTokenService>();

// --- 5. MediatR & Validation ---
builder.Services.AddMediatR(typeof(CreatePackageCommandHandler).Assembly);
// builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePackageCommandValidator>();

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SaveChangesBehavior<,>));

// --- Global Exception Handling ---
builder.Services.AddExceptionHandler<LogiCore.Api.Middlewares.GlobalExceptionHandler>();
builder.Services.AddProblemDetails(); 

var app = builder.Build();

// --- Middlewares Pipeline  ---


app.UseExceptionHandler(); 

// Enable Swagger UI for all environments (including Production)
app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();
// app.UseHttpsRedirection();

// Prometheus HTTP metrics (requests) and scrape endpoint
app.UseHttpMetrics();
app.MapMetrics();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --- Seed admin user (development convenience) ---
try
{
    using (var scope = app.Services.CreateScope())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        const string adminEmail = "admin@gmail.com";
        // Must respect Identity password policy (RequiredLength = 6)
        const string adminPassword = "admin123";
        const string adminRole = "Admin";

        if (!roleManager.RoleExistsAsync(adminRole).GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole(adminRole)).GetAwaiter().GetResult();
        }

        var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                CreatedAt = DateTime.UtcNow
            };

            var createResult = userManager.CreateAsync(adminUser, adminPassword).GetAwaiter().GetResult();
            if (createResult.Succeeded)
            {
                userManager.AddToRoleAsync(adminUser, adminRole).GetAwaiter().GetResult();
                Log.Logger?.Information("Seeded admin user {Email}", adminEmail);
            }
            else
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                Log.Logger?.Warning("Failed to seed admin user {Email}: {Errors}", adminEmail, errors);
            }
        }
        else
        {
            if (!userManager.IsInRoleAsync(adminUser, adminRole).GetAwaiter().GetResult())
            {
                userManager.AddToRoleAsync(adminUser, adminRole).GetAwaiter().GetResult();
            }
        }
    }
}
catch (Exception ex)
{
    Log.Logger?.Error(ex, "Seeding admin user failed");
}

// Seed addresses into Redis (development convenience)
try
{
    using (var scope = app.Services.CreateScope())
    {
            var redis = scope.ServiceProvider.GetService<LogiCore.Application.Common.Interfaces.IAddressAutocompleteRepository>();
        var locationRepo = scope.ServiceProvider.GetService<LogiCore.Application.Common.Interfaces.Persistence.ILocationRepository>();
        if (redis != null && locationRepo != null)
        {
            var locations = locationRepo.GetAllAsync().GetAwaiter().GetResult();
            var addresses = locations.Select(l => string.Join(' ', new[] { l.AddressLine1, l.AddressLine2 ?? string.Empty, l.City, l.State ?? string.Empty, l.PostalCode }).Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();
            redis.SeedAsync(addresses).GetAwaiter().GetResult();
            Log.Logger?.Information("Seeded {Count} addresses into Redis", addresses.Count());
        }
    }
}
catch (Exception ex)
{
    Log.Logger?.Warning(ex, "Seeding addresses into Redis failed");
}

app.Run();
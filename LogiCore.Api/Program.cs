using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LogiCore.Infrastructure.Persistence;
using LogiCore.Domain.Entities;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using LogiCore.Application.Features.Packages;
using MediatR;
using LogiCore.Application.Common.Behaviors;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog early so startup logs are captured
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add controllers and AutoMapper
builder.Services.AddControllers(options =>
{
    options.Filters.Add<LogiCore.Api.Filters.ResultActionFilter>();
});
builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(CreatePackageCommandHandler).Assembly);

// Persistence and application wiring
builder.Services.AddDbContext<LogiCoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    // Configure password settings as needed
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<LogiCoreDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSection.GetValue<string>("SecretKey") ?? string.Empty;
var issuer = jwtSection.GetValue<string>("Issuer") ?? string.Empty;
var audience = jwtSection.GetValue<string>("Audience") ?? string.Empty;

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
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Bind JwtSettings for injection
builder.Services.Configure<LogiCore.Application.Common.Models.JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
// Register JwtProvider implementation
builder.Services.AddSingleton<LogiCore.Application.Common.Interfaces.Security.IJwtProvider, LogiCore.Infrastructure.Security.JwtProvider>();

// Current user service (bridge from HTTP to Application layer)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<LogiCore.Application.Common.Interfaces.Security.ICurrentUserService, LogiCore.Infrastructure.Security.CurrentUserService>();

builder.Services.AddScoped<IPackageRepository, SqlPackageRepository>();

// UnitOfWork: centralize commits
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePackageCommandValidator>();

// MediatR
builder.Services.AddMediatR(typeof(CreatePackageCommandHandler).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

// Commit changes after handlers
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LogiCore.Application.Common.Behaviors.SaveChangesBehavior<,>));

// Global exception handling
builder.Services.AddExceptionHandler<LogiCore.Api.Middlewares.GlobalExceptionHandler>();
builder.Services.AddProblemDetails(); // Necesario para que el handler funcione bien

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        // Esto añade el "chisme" técnico (Exception) solo si estamos en Desarrollo
        if (builder.Environment.IsDevelopment() && context.Exception != null)
        {
            context.ProblemDetails.Extensions["exception"] = context.Exception.ToString();
        }
        
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});

var app = builder.Build();

// Use custom global exception handler middleware
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Log HTTP requests (structured)
app.UseSerilogRequestLogging();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

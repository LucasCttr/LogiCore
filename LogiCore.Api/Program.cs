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

// --- Persistence ---
builder.Services.AddDbContext<LogiCoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPackageRepository, SqlPackageRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Infrastructure services
builder.Services.AddTransient<LogiCore.Application.Common.Interfaces.INotificationService, LogiCore.Infrastructure.Services.ConsoleNotificationService>();
builder.Services.AddTransient<LogiCore.Application.Common.Interfaces.IEmailService, LogiCore.Infrastructure.Services.SmtpEmailService>();
builder.Services.AddTransient<LogiCore.Domain.Common.Interfaces.ICostCalculator, LogiCore.Infrastructure.Services.StandardCostCalculator>();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
using Microsoft.EntityFrameworkCore;
using LogiCore.Infrastructure.Persistence;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using LogiCore.Application.Features.Packages;
using MediatR;
using LogiCore.Application.Common.Behaviors;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<IPackageRepository, SqlPackageRepository>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePackageCommandValidator>();

// MediatR
builder.Services.AddMediatR(typeof(CreatePackageCommandHandler).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

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

// 2. En la parte del pipeline (después del builder.Build)
// IMPORTANTE: Debe ir antes de MapControllers o cualquier endpoint
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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

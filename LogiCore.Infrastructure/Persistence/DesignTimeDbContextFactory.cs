using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LogiCore.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LogiCoreDbContext>
{
    public LogiCoreDbContext CreateDbContext(string[] args)
    {
        // Try to locate API project appsettings (assume workspace layout)
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../LogiCore.Api");

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Prefer DATABASE_URL (e.g., provided by Railway); otherwise use DefaultConnection
        var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        string? conn = null;
        if (!string.IsNullOrWhiteSpace(dbUrl))
        {
            var npgsqlBuilder = new Npgsql.NpgsqlConnectionStringBuilder(dbUrl);
            conn = npgsqlBuilder.ToString();
        }
        else
        {
            conn = config.GetConnectionString("DefaultConnection");
        }

        if (string.IsNullOrWhiteSpace(conn))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Set it in LogiCore.Api/appsettings.Development.json or provide DATABASE_URL env var.");

        var optionsBuilder = new DbContextOptionsBuilder<LogiCoreDbContext>();
        optionsBuilder.UseNpgsql(conn);
        return new LogiCoreDbContext(optionsBuilder.Options);
    }
}

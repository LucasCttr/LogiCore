using Microsoft.EntityFrameworkCore;
using LogiCore.Domain.Entities;

namespace LogiCore.Infrastructure.Persistence;

public class LogiCoreDbContext : DbContext
{
    public LogiCoreDbContext(DbContextOptions<LogiCoreDbContext> options)
        : base(options) { }

    public DbSet<Package> Packages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LogiCoreDbContext).Assembly);
    }
}
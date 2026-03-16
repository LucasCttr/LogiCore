using Microsoft.EntityFrameworkCore;
using LogiCore.Domain.Entities;

namespace LogiCore.Infrastructure.Persistence;

public class LogiCoreDbContext : DbContext
{
    public LogiCoreDbContext(DbContextOptions<LogiCoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Package> Packages { get; set; } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration
        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TrackingNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.RecipientName).IsRequired().HasMaxLength(200);
        });
    }
}
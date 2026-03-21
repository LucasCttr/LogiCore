using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LogiCore.Domain.Entities;

namespace LogiCore.Infrastructure.Persistence;

public class LogiCoreDbContext : IdentityDbContext<ApplicationUser>
{
    public LogiCoreDbContext(DbContextOptions<LogiCoreDbContext> options) : base(options) { }

    public DbSet<Package> Packages => Set<Package>();
    public DbSet<PackageStatusHistory> PackageStatusHistories => Set<PackageStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Search and apply all configurations in the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LogiCoreDbContext).Assembly);
        
        // Map Dimensions as owned value object stored in Package table
        modelBuilder.Entity<Package>().OwnsOne(typeof(LogiCore.Domain.ValueObjects.Dimensions), "_dimensions", owned =>
        {
            owned.Property<decimal>("LengthCm").HasColumnName("LengthCm");
            owned.Property<decimal>("WidthCm").HasColumnName("WidthCm");
            owned.Property<decimal>("HeightCm").HasColumnName("HeightCm");
        });
        base.OnModelCreating(modelBuilder);
    }
}
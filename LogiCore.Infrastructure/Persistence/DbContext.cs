using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LogiCore.Domain.Entities;

namespace LogiCore.Infrastructure.Persistence;

public class LogiCoreDbContext : IdentityDbContext<ApplicationUser>
{
    public LogiCoreDbContext(DbContextOptions<LogiCoreDbContext> options) : base(options) { }

    public DbSet<Package> Packages => Set<Package>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<DriverDetails> DriverDetails => Set<DriverDetails>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<PackageStatusHistory> PackageStatusHistories => Set<PackageStatusHistory>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Search and apply all configurations in the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LogiCoreDbContext).Assembly);
        
        // Value object mappings are applied from configuration classes in the assembly
        base.OnModelCreating(modelBuilder);
    }
}
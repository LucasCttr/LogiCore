using System;
using Microsoft.EntityFrameworkCore;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Domain.Entities;
using LogiCore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace LogiCore.Infrastructure.Repositories;

public class SqlShipmentRepository : IShipmentRepository
{
    private readonly LogiCoreDbContext _context;
    private readonly ILogger<SqlShipmentRepository> _logger;

    public SqlShipmentRepository(LogiCoreDbContext context, ILogger<SqlShipmentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Shipment> AddAsync(Shipment shipment)
    {
        var entry = await _context.Shipments.AddAsync(shipment);
        return entry.Entity;
    }

    public async Task<IEnumerable<Shipment>> GetAllAsync()
    {
        return await _context.Shipments
            .AsNoTracking()
            .Include(s => s.Packages)
            .Include(s => s.Vehicle)
            .Include(s => s.Driver)
            .ToListAsync();
    }

    public async Task<IEnumerable<Shipment>> GetByDriverIdAsync(Guid driverId)
    {
        return await _context.Shipments
            .AsNoTracking()
            .Include(s => s.Packages)
            .Include(s => s.Vehicle)
            .Include(s => s.Driver)
            .Where(s => s.DriverId == driverId)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Shipment> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? sortBy = null, string? sortDir = null, string? status = null, string? q = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _context.Shipments
            .AsNoTracking()
            .Include(s => s.Packages)
            .Include(s => s.Vehicle)
            .Include(s => s.Driver)
            .AsQueryable();

        // Filtering by status (single enum, or InProgress = Loading | Dispatched for list tabs)
        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusTrim = status.Trim();
            if (string.Equals(statusTrim, "InProgress", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.Status == ShipmentStatus.Loading || s.Status == ShipmentStatus.Dispatched);
            }
            else if (string.Equals(statusTrim, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.Status == ShipmentStatus.Arrived || s.Status == ShipmentStatus.Delivered);
            }
            else if (Enum.TryParse<ShipmentStatus>(statusTrim, true, out var statusEnum))
            {
                query = query.Where(s => s.Status == statusEnum);
            }
        }

        // Simple search across route code, vehicle plate, driver name, or package tracking number
        if (!string.IsNullOrWhiteSpace(q))
        {
            var qTrim = q.Trim();
            query = query.Where(s =>
                s.RouteCode.Contains(qTrim) ||
                (s.Vehicle != null && s.Vehicle.Plate.Contains(qTrim)) ||
                (s.Driver != null && s.Driver.Name.Contains(qTrim)) ||
                s.Packages.Any(p => p.TrackingNumber.Contains(qTrim))
            );
        }

        // Total before paging
        var total = await query.CountAsync();

        // Sorting
        if (string.IsNullOrWhiteSpace(sortBy)) sortBy = "routeCode";
        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        // Apply simple sorting switches for known fields
        query = sortBy.ToLowerInvariant() switch
        {
            "status" => desc ? query.OrderByDescending(s => s.Status) : query.OrderBy(s => s.Status),
            "estimateddelivery" => desc ? query.OrderByDescending(s => s.EstimatedDelivery) : query.OrderBy(s => s.EstimatedDelivery),
            "createdat" => desc ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            "vehicleplate" => desc ? query.OrderByDescending(s => s.Vehicle!.Plate) : query.OrderBy(s => s.Vehicle!.Plate),
            _ => desc ? query.OrderByDescending(s => s.RouteCode) : query.OrderBy(s => s.RouteCode),
        };

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, total);
    }

    public async Task<Shipment?> GetByIdAsync(Guid id)
    {
        Console.WriteLine($"[GetByIdAsync] Loading shipment {id}");
        var shipment = await _context.Shipments
            .Include(s => s.Packages)
            .Include(s => s.Vehicle)
            .Include(s => s.Driver)
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (shipment != null)
        {
            Console.WriteLine($"[GetByIdAsync] Shipment {id} found. Packages: {shipment.Packages.Count}");
            foreach (var pkg in shipment.Packages)
            {
                Console.WriteLine($"[GetByIdAsync]   - Package {pkg.Id}: Status = {pkg.Status}");
            }
        }
        else
        {
            Console.WriteLine($"[GetByIdAsync] Shipment {id} not found");
        }
        
        return shipment;
    }

    public async Task<Shipment?> GetByPackageIdAsync(Guid packageId) =>
        await _context.Shipments
            .Include(s => s.Packages)
            .Include(s => s.Vehicle)
            .Include(s => s.Driver)
            .FirstOrDefaultAsync(s => s.Packages.Any(p => p.Id == packageId));

    public Task<Shipment> UpdateAsync(Shipment shipment)
    {
        Console.WriteLine($"[UpdateAsync] Called for shipment {shipment.Id} with {shipment.Packages.Count} packages");
        
        foreach (var pkg in shipment.Packages)
        {
            Console.WriteLine($"[UpdateAsync]   - Package {pkg.Id}: Status = {pkg.Status}");
        }

        _context.Shipments.Update(shipment);
        
        Console.WriteLine($"[UpdateAsync] Marked shipment as Updated in context");
        return Task.FromResult(shipment);
    }

    public async Task SyncPackagesInDatabaseAsync(Shipment shipment)
    {
        _logger.LogInformation($"[SqlShipmentRepository.SyncPackagesInDatabaseAsync] Syncing packages for shipment {shipment.Id}, Packages count: {shipment.Packages.Count}");
        Console.WriteLine($"[SyncPackages] Starting sync for shipment {shipment.Id}");
        Console.WriteLine($"[SyncPackages] Shipment Type: {shipment.Type}");
        Console.WriteLine($"[SyncPackages] Total packages: {shipment.Packages.Count}");
        
        var packageIds = shipment.Packages.Select(p => p.Id).ToList();
        if (!packageIds.Any())
        {
            Console.WriteLine($"[SyncPackages] No packages to sync");
            _logger.LogInformation($"[SqlShipmentRepository.SyncPackagesInDatabaseAsync] No packages to sync");
            return;
        }

        Console.WriteLine($"[SyncPackages] Package IDs to process: {string.Join(", ", packageIds)}");

        // Get package Ids that need to be updated to specific statuses
        var packageIdsToUpdateToInTransit = shipment.Packages
            .Where(p => p.Status == PackageStatus.InTransit)
            .Select(p => p.Id)
            .ToList();

        var packageIdsToUpdateToAtDepot = shipment.Packages
            .Where(p => p.Status == PackageStatus.AtDepot)
            .Select(p => p.Id)
            .ToList();

        Console.WriteLine($"[SyncPackages] Packages to update to InTransit: {packageIdsToUpdateToInTransit.Count}");
        foreach (var id in packageIdsToUpdateToInTransit)
        {
            Console.WriteLine($"[SyncPackages]   - {id}");
        }

        Console.WriteLine($"[SyncPackages] Packages to update to AtDepot: {packageIdsToUpdateToAtDepot.Count}");

        var now = DateTime.UtcNow;

        // Update packages to InTransit
        if (packageIdsToUpdateToInTransit.Any())
        {
            Console.WriteLine($"[SyncPackages] Executing ExecuteUpdateAsync for {packageIdsToUpdateToInTransit.Count} packages to InTransit");
            _logger.LogInformation($"[SqlShipmentRepository.SyncPackagesInDatabaseAsync] Updating {packageIdsToUpdateToInTransit.Count} packages to InTransit");
            
            // First verify packages exist
            var existingCount = await _context.Packages
                .Where(p => packageIdsToUpdateToInTransit.Contains(p.Id))
                .CountAsync();
            Console.WriteLine($"[SyncPackages] Found {existingCount} packages in DB to update");
            
            var updated = await _context.Packages
                .Where(p => packageIdsToUpdateToInTransit.Contains(p.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Status, PackageStatus.InTransit)
                    .SetProperty(p => p.LastUpdatedAt, now)
                );
            
            Console.WriteLine($"[SyncPackages] ExecuteUpdateAsync returned {updated} affected rows");
        }

        // Update packages to AtDepot
        if (packageIdsToUpdateToAtDepot.Any())
        {
            Console.WriteLine($"[SyncPackages] Executing ExecuteUpdateAsync for {packageIdsToUpdateToAtDepot.Count} packages to AtDepot");
            _logger.LogInformation($"[SqlShipmentRepository.SyncPackagesInDatabaseAsync] Updating {packageIdsToUpdateToAtDepot.Count} packages to AtDepot");
            
            var updated = await _context.Packages
                .Where(p => packageIdsToUpdateToAtDepot.Contains(p.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Status, PackageStatus.AtDepot)
                    .SetProperty(p => p.LastUpdatedAt, now)
                );
            
            Console.WriteLine($"[SyncPackages] Updated {updated} packages to AtDepot");
        }

        Console.WriteLine($"[SyncPackages] Sync complete");
        _logger.LogInformation($"[SqlShipmentRepository.SyncPackagesInDatabaseAsync] Sync complete");
    }
}

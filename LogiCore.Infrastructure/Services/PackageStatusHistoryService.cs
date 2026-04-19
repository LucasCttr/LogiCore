using System;
using System.Threading;
using System.Threading.Tasks;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Domain.Entities;
using LogiCore.Infrastructure.Persistence;

namespace LogiCore.Infrastructure.Services;

/// <summary>
/// Service for managing package status history records.
/// Records delivery/collection attempt failures without changing package status.
/// </summary>
public class PackageStatusHistoryService : IPackageStatusHistoryService
{
    private readonly LogiCoreDbContext _context;

    public PackageStatusHistoryService(LogiCoreDbContext context)
    {
        _context = context;
    }

    public async Task AddAttemptFailedHistoryAsync(
        Guid packageId,
        string? reason,
        string? userId = null,
        int? locationId = null,
        Guid? shipmentId = null,
        CancellationToken cancellationToken = default)
    {
        var history = new PackageStatusHistory
        {
            Id = Guid.NewGuid(),
            PackageId = packageId,
            FromStatus = (int)Domain.Entities.PackageStatus.Pending, // Remains Pending
            ToStatus = (int)Domain.Entities.PackageStatus.Pending, // Remains Pending
            OccurredAt = DateTime.UtcNow,
            UserId = userId,
            LocationId = locationId,
            ShipmentId = shipmentId,
            Notes = $"Delivery attempt failed: {reason ?? "No recipient available"}",
            EmployeeId = userId,
            InternalNotes = $"Delivery attempt failed: {reason ?? "No recipient available"}"
        };

        await _context.PackageStatusHistories.AddAsync(history, cancellationToken);
    }
}

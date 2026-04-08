using AutoMapper;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using MediatR;

namespace LogiCore.Application.Features.Shipment;

public class BulkDeliverToCenterCommandHandler : IRequestHandler<BulkDeliverToCenterCommand, Result<bool>>
{
    private readonly IPackageRepository _packageRepository;

    public BulkDeliverToCenterCommandHandler(IPackageRepository packageRepository)
    {
        _packageRepository = packageRepository;
    }

    public async Task<Result<bool>> Handle(BulkDeliverToCenterCommand request, CancellationToken cancellationToken)
    {
        if (request.PackageIds == null || !request.PackageIds.Any())
            return Result<bool>.Failure("No se proporcionaron IDs de paquetes.");

        // 1. Fetch all packages in a single query
        var packages = (await _packageRepository.GetManyByIdsAsync(request.PackageIds)).ToList();

        // 2. Filter those that are at depot and update in memory
        var packagesToUpdate = packages
            .Where(p => p.Status == Domain.Entities.PackageStatus.AtDepot)
            .ToList();

        if (!packagesToUpdate.Any())
            return Result<bool>.Success(true);

        foreach (var package in packagesToUpdate)
        {
            package.DeliverToCenter();
        }

        // 3. Save all changes in a single batch
        await _packageRepository.UpdateRangeAsync(packagesToUpdate);

        return Result<bool>.Success(true);
    }
}

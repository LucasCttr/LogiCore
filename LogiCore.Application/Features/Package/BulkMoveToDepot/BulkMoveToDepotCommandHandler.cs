using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using MediatR;

namespace LogiCore.Application.Features.Packages;

public class BulkMoveToDepotCommandHandler : IRequestHandler<BulkMoveToDepotCommand, Result<bool>>
{
    private readonly IPackageRepository _packageRepository;

    public BulkMoveToDepotCommandHandler(IPackageRepository packageRepository)
    {
        _packageRepository = packageRepository;
    }

    public async Task<Result<bool>> Handle(BulkMoveToDepotCommand request, CancellationToken cancellationToken)
    {
        if (request.PackageIds == null || !request.PackageIds.Any())
            return Result<bool>.Failure("No se proporcionaron IDs de paquetes.");

        var packages = (await _packageRepository.GetManyByIdsAsync(request.PackageIds)).ToList();

        // Allow moving Pending or InTransit packages to depot
        var packagesToMove = packages
            .Where(p => p.Status == Domain.Entities.PackageStatus.Pending || p.Status == Domain.Entities.PackageStatus.InTransit)
            .ToList();

        if (!packagesToMove.Any())
            return Result<bool>.Success(true);

        foreach (var package in packagesToMove)
        {
            package.MoveToDepot();
        }

        await _packageRepository.UpdateRangeAsync(packagesToMove);

        return Result<bool>.Success(true);
    }
}

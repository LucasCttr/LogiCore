using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using MediatR;

namespace LogiCore.Application.Features.Packages;

public class BulkCancelPackagesCommandHandler : IRequestHandler<BulkCancelPackagesCommand, Result<bool>>
{
    private readonly IPackageRepository _packageRepository;

    public BulkCancelPackagesCommandHandler(IPackageRepository packageRepository)
    {
        _packageRepository = packageRepository;
    }

    public async Task<Result<bool>> Handle(BulkCancelPackagesCommand request, CancellationToken cancellationToken)
    {
        if (request.PackageIds == null || !request.PackageIds.Any())
            return Result<bool>.Failure("No package ids provided.");

        var packages = (await _packageRepository.GetManyByIdsAsync(request.PackageIds)).ToList();
        if (!packages.Any()) return Result<bool>.Success(true);

        var toProcess = packages.Where(p => p.Status == Domain.Entities.PackageStatus.InTransit || p.Status == Domain.Entities.PackageStatus.AtDepot).ToList();
        if (!toProcess.Any()) return Result<bool>.Success(true);

        foreach (var pkg in toProcess)
        {
            if (request.ReturnToOrigin)
            {
                pkg.ReturnToOrigin();
            }
            else
            {
                // Use domain Cancel to respect state rules
                pkg.Cancel();
            }
        }

        await _packageRepository.UpdateRangeAsync(toProcess);
        return Result<bool>.Success(true);
    }
}

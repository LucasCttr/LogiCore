using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Common.Exceptions;
using MediatR;

namespace LogiCore.Application.Features.Packages;

public class BulkCancelPackagesCommandHandler : IRequestHandler<BulkCancelPackagesCommand, Result<bool>>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BulkCancelPackagesCommandHandler(IPackageRepository packageRepository, IUnitOfWork unitOfWork)
    {
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(BulkCancelPackagesCommand request, CancellationToken cancellationToken)
    {
        if (request.PackageIds == null || !request.PackageIds.Any())
            return Result<bool>.Failure("No package IDs provided.", ErrorType.Validation);

        try
        {
            var packages = (await _packageRepository.GetManyByIdsAsync(request.PackageIds)).ToList();
            if (!packages.Any())
                return Result<bool>.Success(true);

            // Filter packages that can be canceled/returned
            var toProcess = packages
                .Where(p => p.Status == Domain.Entities.PackageStatus.InTransit || 
                           p.Status == Domain.Entities.PackageStatus.AtDepot)
                .ToList();

            if (!toProcess.Any())
                return Result<bool>.Success(true);

            // Apply the requested action based on ReturnToOrigin flag
            foreach (var pkg in toProcess)
            {
                if (request.ReturnToOrigin)
                {
                    pkg.ReturnToOrigin();
                }
                else
                {
                    pkg.Cancel();
                }
            }

            // Persist changes with transaction
            await _packageRepository.UpdateRangeAsync(toProcess);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (DomainException ex)
        {
            return Result<bool>.Failure(ex.Message, ErrorType.Conflict);
        }
    }
}

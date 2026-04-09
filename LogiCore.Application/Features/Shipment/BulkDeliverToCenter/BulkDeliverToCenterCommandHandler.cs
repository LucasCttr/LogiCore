using AutoMapper;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Common.Exceptions;
using MediatR;

namespace LogiCore.Application.Features.Shipment;

public class BulkDeliverToCenterCommandHandler : IRequestHandler<BulkDeliverToCenterCommand, Result<bool>>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BulkDeliverToCenterCommandHandler(IPackageRepository packageRepository, IUnitOfWork unitOfWork)
    {
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(BulkDeliverToCenterCommand request, CancellationToken cancellationToken)
    {
        if (request.PackageIds == null || !request.PackageIds.Any())
            return Result<bool>.Failure("No package IDs provided.", ErrorType.Validation);

        try
        {
            // Fetch all packages
            var packages = (await _packageRepository.GetManyByIdsAsync(request.PackageIds)).ToList();
            if (!packages.Any())
                return Result<bool>.Success(true);

            // Filter and update only those at depot
            var packagesToUpdate = packages
                .Where(p => p.Status == Domain.Entities.PackageStatus.AtDepot)
                .ToList();

            if (!packagesToUpdate.Any())
                return Result<bool>.Success(true);

            foreach (var package in packagesToUpdate)
            {
                package.DeliverToCenter();
            }

            // Save changes with transaction
            await _packageRepository.UpdateRangeAsync(packagesToUpdate);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (DomainException ex)
        {
            return Result<bool>.Failure(ex.Message, ErrorType.Conflict);
        }
    }
}

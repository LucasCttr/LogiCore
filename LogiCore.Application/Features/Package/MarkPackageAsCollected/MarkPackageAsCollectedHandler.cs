using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Features.Package.MarkPackageAsCollected;

/// <summary>
/// Handler for marking a single package as collected during a Pickup shipment.
/// </summary>
public class MarkPackageAsCollectedHandler : IRequestHandler<MarkPackageAsCollectedCommand, Result<bool>>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkPackageAsCollectedHandler(
        IPackageRepository packageRepository,
        IUnitOfWork unitOfWork)
    {
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(MarkPackageAsCollectedCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validate command
            if (request.PackageId == Guid.Empty)
                return Result<bool>.Failure("Invalid package ID.", ErrorType.Validation);

            // 2. Get the package
            var package = await _packageRepository.GetByIdAsync(request.PackageId);
            if (package == null)
                return Result<bool>.Failure("Package not found.", ErrorType.NotFound);

            // 3. Verify package is in Pending status (can be collected)
            if (package.Status != PackageStatus.Pending)
                return Result<bool>.Failure(
                    $"Cannot collect a package in {package.Status} status. Only pending packages can be collected.",
                    ErrorType.Conflict);

            // 4. Mark as collected (using domain method)
            package.Collect();

            // 5. Persist changes
            await _packageRepository.UpdateAsync(package);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (DomainException ex)
        {
            return Result<bool>.Failure(ex.Message, ErrorType.Validation);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                $"An error occurred while marking package as collected: {ex.Message}",
                ErrorType.None);
        }
    }
}

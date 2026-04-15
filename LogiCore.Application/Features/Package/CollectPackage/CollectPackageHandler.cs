using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Application.Features.Package.CollectPackage;

/// <summary>
/// Handler for collecting/picking up a package from seller location.
/// Transitions from Pending → InTransit (loaded in driver's vehicle).
/// </summary>
public class CollectPackageHandler : IRequestHandler<CollectPackageCommand, Result<bool>>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CollectPackageHandler(IPackageRepository packageRepository, IUnitOfWork unitOfWork)
    {
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(CollectPackageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validate command
            if (request.PackageId == Guid.Empty)
                return Result<bool>.Failure("Invalid package ID.", ErrorType.Validation);

            // 2. Get package
            var package = await _packageRepository.GetByIdAsync(request.PackageId);
            if (package == null)
                return Result<bool>.Failure("Package not found.", ErrorType.NotFound);

            // 3. Verify package is in Pending status (ready for collection)
            if (package.Status != LogiCore.Domain.Entities.PackageStatus.Pending)
                return Result<bool>.Failure(
                    $"Cannot collect a package in {package.Status} status. Only pending packages can be collected.",
                    ErrorType.Conflict);

            // 4. Collect package (Pending → InTransit)
            package.StartTransit();

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
                $"An error occurred while collecting package: {ex.Message}",
                ErrorType.None);
        }
    }
}

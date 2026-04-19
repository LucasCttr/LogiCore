using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Application.Features.Package.MarkPackageAsDelivered;

/// <summary>
/// Handler for marking a single package as delivered by the driver.
/// </summary>
public class MarkPackageAsDeliveredHandler : IRequestHandler<MarkPackageAsDeliveredCommand, Result<bool>>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkPackageAsDeliveredHandler(
        IPackageRepository packageRepository,
        IUnitOfWork unitOfWork)
    {
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(MarkPackageAsDeliveredCommand request, CancellationToken cancellationToken)
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

            // 3. Verify package can be delivered
            // Allow: AtDepot (normal flow) or Pending (retry after attempt failed)
            if (package.Status != LogiCore.Domain.Entities.PackageStatus.AtDepot &&
                package.Status != LogiCore.Domain.Entities.PackageStatus.Pending &&
                package.Status != LogiCore.Domain.Entities.PackageStatus.InTransit)
                return Result<bool>.Failure(
                    $"Cannot deliver a package in {package.Status} status.",
                    ErrorType.Conflict);

            // 4. Mark package as delivered (using domain method)
            package.Deliver();

            // 5. TODO: Store delivery metadata (notes, location, timestamp) if needed
            // For now, these are just passed through the command but not persisted
            // You might want to extend the Package entity or create a DeliveryEvent for this

            // 6. Persist changes
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
                $"An error occurred while marking package as delivered: {ex.Message}",
                ErrorType.None);
        }
    }
}

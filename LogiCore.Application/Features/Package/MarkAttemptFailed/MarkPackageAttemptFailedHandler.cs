using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Interfaces.Security;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Application.Features.Package.MarkAttemptFailed;

public class MarkPackageAttemptFailedHandler : IRequestHandler<MarkPackageAttemptFailedCommand, Result<bool>>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IPackageStatusHistoryService _historyService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public MarkPackageAttemptFailedHandler(
        IPackageRepository packageRepository,
        IPackageStatusHistoryService historyService,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _packageRepository = packageRepository;
        _historyService = historyService;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(MarkPackageAttemptFailedCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validate package exists
            var package = await _packageRepository.GetByIdAsync(request.PackageId);
            if (package == null)
                return Result<bool>.Failure("Package not found.", ErrorType.NotFound);

            // 2. Record the failed attempt in history (without changing package status)
            var reason = request.Reason ?? "No recipient available";
            await _historyService.AddAttemptFailedHistoryAsync(
                request.PackageId,
                reason,
                _currentUserService?.UserId,
                cancellationToken: cancellationToken);

            // 3. Commit changes
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (DomainException ex)
        {
            return Result<bool>.Failure(ex.Message, ErrorType.Conflict);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error marking attempt as failed: {ex.Message}", ErrorType.None);
        }
    }
}

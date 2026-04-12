using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Entities;
using MediatR;

namespace LogiCore.Application.Features.Package.MoveToDepot;

public class MovePackageToDepotCommandHandler : IRequestHandler<MovePackageToDepotCommand, Result<bool>>
{
    private readonly IPackageRepository _packageRepository;

    public MovePackageToDepotCommandHandler(IPackageRepository packageRepository)
    {
        _packageRepository = packageRepository;
    }

    public async Task<Result<bool>> Handle(MovePackageToDepotCommand request, CancellationToken cancellationToken)
    {
        // Get the package
        var package = await _packageRepository.GetByIdAsync(request.PackageId);
        if (package == null)
            return Result<bool>.Failure("Package not found.", ErrorType.NotFound);

        // Validate package is in a valid status to move to depot
        // Allow moving Pending or InTransit packages to depot
        if (package.Status != PackageStatus.Pending && package.Status != PackageStatus.InTransit)
            return Result<bool>.Failure($"Package cannot be moved to depot from status '{package.Status}'.", ErrorType.Validation);

        // Move to depot
        package.MoveToDepot();

        // Save
        await _packageRepository.UpdateAsync(package);

        return Result<bool>.Success(true);
    }
}

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.Common.Interfaces.Persistence;

namespace LogiCore.Application.Features.Vehicle.DeleteVehicle;

public class DeleteVehicleHandler : IRequestHandler<DeleteVehicleCommand, Result<bool>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVehicleHandler(IVehicleRepository vehicleRepository, IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        var existing = await _vehicleRepository.GetByIdAsync(request.Id);
        if (existing is null) return Result<bool>.Failure("Vehicle not found.");

        existing.SetActive(false);
        await _vehicleRepository.UpdateAsync(existing);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

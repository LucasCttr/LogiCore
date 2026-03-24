using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Application.Common.Interfaces.Persistence;

namespace LogiCore.Application.Features.Vehicle.UpdateVehicle;

public class UpdateVehicleHandler : IRequestHandler<UpdateVehicleCommand, Result<VehicleDto>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateVehicleHandler(IVehicleRepository vehicleRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<VehicleDto>> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var existing = await _vehicleRepository.GetByIdAsync(request.Id);
        if (existing is null) return Result<VehicleDto>.Failure("Vehicle not found.");

        existing = _mapper.Map(request, existing);
        await _vehicleRepository.UpdateAsync(existing);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<VehicleDto>.Success(_mapper.Map<VehicleDto>(existing));
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Features.Vehicle.UpdateStatus;

public class UpdateVehicleStatusHandler : IRequestHandler<UpdateVehicleStatusCommand, Result<LogiCore.Application.DTOs.VehicleDto>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateVehicleStatusHandler(IVehicleRepository vehicleRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<LogiCore.Application.DTOs.VehicleDto>> Handle(UpdateVehicleStatusCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.Id);
        if (vehicle is null) return Result<LogiCore.Application.DTOs.VehicleDto>.Failure("Vehicle not found.", ErrorType.NotFound);

        if (!Enum.TryParse<VehicleStatus>(request.Status, true, out var status))
            return Result<LogiCore.Application.DTOs.VehicleDto>.Failure("Invalid status.");

        vehicle.SetStatus(status);

        await _vehicleRepository.UpdateAsync(vehicle);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<LogiCore.Application.DTOs.VehicleDto>.Success(_mapper.Map<LogiCore.Application.DTOs.VehicleDto>(vehicle));
    }
}

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using System;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Application.Common.Interfaces.Persistence;
using AutoMapper;

namespace LogiCore.Application.Features.Vehicle.CreateVehicle;

public class CreateVehicleHandler : IRequestHandler<CreateVehicleCommand, Result<VehicleDto>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateVehicleHandler(IVehicleRepository vehicleRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<VehicleDto>> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = Domain.Entities.Vehicle.Create(request.Plate, request.MaxWeightCapacity, request.MaxVolumeCapacity);
        await _vehicleRepository.AddAsync(vehicle);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result<VehicleDto>.Success(_mapper.Map<VehicleDto>(vehicle));
    }
}

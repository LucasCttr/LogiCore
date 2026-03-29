using AutoMapper;
using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Features.Shipment.CreateShipment;

public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, Result<ShipmentDto>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CreateShipmentCommandHandler(IShipmentRepository shipmentRepository, IVehicleRepository vehicleRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _shipmentRepository = shipmentRepository;
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ShipmentDto>> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Load vehicle from repository to obtain authoritative capacities
            var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
            if (vehicle == null)
                return Result<ShipmentDto>.Failure("Vehicle not found.");
            if (!vehicle.IsActive)
                return Result<ShipmentDto>.Failure("Vehicle is not active.");

            var shipment = LogiCore.Domain.Entities.Shipment.Create(request.RouteCode, request.VehicleId, vehicle.MaxWeightCapacity, vehicle.MaxVolumeCapacity, request.EstimatedDelivery);
            var added = await _shipmentRepository.AddAsync(shipment);

            // Commit to persist the new shipment and publish any domain events
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<ShipmentDto>.Success(_mapper.Map<ShipmentDto>(added));
        }
        catch (LogiCore.Domain.Common.Exceptions.DomainException ex)
        {
            return Result<ShipmentDto>.Failure(ex.Message);
        }
    }
}

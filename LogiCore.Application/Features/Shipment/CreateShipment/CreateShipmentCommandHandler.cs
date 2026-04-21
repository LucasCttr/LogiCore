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
    private readonly IDriverRepository _driverRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CreateShipmentCommandHandler(
        IShipmentRepository shipmentRepository,
        IVehicleRepository vehicleRepository,
        IDriverRepository driverRepository,
        IPackageRepository packageRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _shipmentRepository = shipmentRepository;
        _vehicleRepository = vehicleRepository;
        _driverRepository = driverRepository;
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ShipmentDto>> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"\n========== [CreateShipment] START ==========");
            Console.WriteLine($"[CreateShipment] OriginLocationId: {request.OriginLocationId}");
            Console.WriteLine($"[CreateShipment] DestinationLocationId: {request.DestinationLocationId}");
            Console.WriteLine($"[CreateShipment] Explicit Type: {request.Type}");
            
            // Validate vehicle exists and is active
            var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
            if (vehicle == null)
                return Result<ShipmentDto>.Failure("Vehicle not found.", ErrorType.NotFound);
            if (!vehicle.IsActive)
                return Result<ShipmentDto>.Failure("Vehicle is not active.", ErrorType.Validation);

            // Validate driver exists and is active
            var driver = await _driverRepository.GetByIdAsync(request.DriverId);
            if (driver == null)
                return Result<ShipmentDto>.Failure("Driver not found.", ErrorType.NotFound);
            if (!driver.IsActive)
                return Result<ShipmentDto>.Failure("Driver is not active.", ErrorType.Validation);

            // Validate all packages exist
            var packages = (await _packageRepository.GetManyByIdsAsync(request.PackageIds)).ToList();
            if (packages.Count != request.PackageIds.Count)
                return Result<ShipmentDto>.Failure("One or more packages not found.", ErrorType.NotFound);

            // Generate route code
            var routeCode = $"ROUTE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            // Create shipment
            Console.WriteLine($"[CreateShipment] Creating shipment with type parameter: {request.Type}");
            var shipment = Domain.Entities.Shipment.Create(
                routeCode,
                request.VehicleId,
                vehicle.MaxWeightCapacity,
                vehicle.MaxVolumeCapacity,
                request.EstimatedDelivery,
                request.OriginLocationId,
                request.DestinationLocationId,
                request.Type);

            Console.WriteLine($"[CreateShipment] Shipment created with Type: {shipment.Type}");
            
            // Assign driver and add packages
            shipment.AssignDriver(request.DriverId);
            shipment.AddPackages(packages);

            // Persist
            var added = await _shipmentRepository.AddAsync(shipment);
            await _unitOfWork.CommitAsync(cancellationToken);

            Console.WriteLine($"[CreateShipment] ✓ Shipment saved. Final Type: {added.Type}");
            Console.WriteLine($"========== [CreateShipment] COMPLETED ==========\n");
            
            return Result<ShipmentDto>.Success(_mapper.Map<ShipmentDto>(added));
        }
        catch (LogiCore.Domain.Common.Exceptions.DomainException ex)
        {
            Console.WriteLine($"[CreateShipment] ❌ DomainException: {ex.Message}");
            Console.WriteLine($"========== [CreateShipment] FAILED ==========\n");
            return Result<ShipmentDto>.Failure(ex.Message, ErrorType.Conflict);
        }
    }
}

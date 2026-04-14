using MediatR;
using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.DTOs;
using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Application.Features.Driver;

public class AssignVehicleToDriverHandler : IRequestHandler<AssignVehicleToDriverCommand, Result<DriverDto>>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IMapper _mapper;

    public AssignVehicleToDriverHandler(
        IDriverRepository driverRepository,
        IVehicleRepository vehicleRepository,
        IMapper mapper)
    {
        _driverRepository = driverRepository;
        _vehicleRepository = vehicleRepository;
        _mapper = mapper;
    }

    public async Task<Result<DriverDto>> Handle(AssignVehicleToDriverCommand request, CancellationToken cancellationToken)
    {
        // Get driver
        var driver = await _driverRepository.GetByIdAsync(request.DriverId);
        if (driver is null)
            return Result<DriverDto>.Failure("Driver not found.", ErrorType.NotFound);

        // If assigning a vehicle, validate it exists
        if (request.VehicleId.HasValue && request.VehicleId.Value != Guid.Empty)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId.Value);
            if (vehicle is null)
                return Result<DriverDto>.Failure("Vehicle not found.", ErrorType.NotFound);
        }

        // Assign vehicle
        driver.AssignVehicle(request.VehicleId);

        // Update driver
        var updated = await _driverRepository.UpdateAsync(driver);

        return Result<DriverDto>.Success(_mapper.Map<DriverDto>(updated));
    }
}

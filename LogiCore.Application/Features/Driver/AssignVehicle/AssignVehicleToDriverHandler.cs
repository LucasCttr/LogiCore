using MediatR;
using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.DTOs;
using LogiCore.Application.Repositories;
using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Application.Features.Driver;

public class AssignVehicleToDriverHandler : IRequestHandler<AssignVehicleToDriverCommand, Result<DriverDto>>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IDriverDetailsRepository _driverDetailsRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IMapper _mapper;

    public AssignVehicleToDriverHandler(
        IDriverRepository driverRepository,
        IDriverDetailsRepository driverDetailsRepository,
        IVehicleRepository vehicleRepository,
        IMapper mapper)
    {
        _driverRepository = driverRepository;
        _driverDetailsRepository = driverDetailsRepository;
        _vehicleRepository = vehicleRepository;
        _mapper = mapper;
    }

    public async Task<Result<DriverDto>> Handle(AssignVehicleToDriverCommand request, CancellationToken cancellationToken)
    {
        // Try to get driver by ID
        var driver = await _driverRepository.GetByIdAsync(request.DriverId);
        
        if (driver is null)
        {
            // If not found, try as DriverDetails ID
            var driverDetails = await _driverDetailsRepository.GetByIdAsync(request.DriverId);
            if (driverDetails is null)
                return Result<DriverDto>.Failure("Driver or DriverDetails not found.", ErrorType.NotFound);
            
            // Try to get existing Driver by ApplicationUserId
            driver = await _driverRepository.GetByApplicationUserIdAsync(driverDetails.UserId);
            if (driver is null)
            {
                // Create Driver from DriverDetails
                var fullName = $"{driverDetails.User?.FirstName} {driverDetails.User?.LastName}".Trim();
                driver = LogiCore.Domain.Entities.Driver.Create(
                    name: fullName,
                    licenseNumber: driverDetails.LicenseNumber,
                    applicationUserId: driverDetails.UserId
                );
                
                // Assign vehicle immediately
                driver.AssignVehicle(request.VehicleId);
                
                // Add driver first
                var created = await _driverRepository.AddAsync(driver);
                
                // Now update DriverDetails with fresh instance
                var freshDriverDetails = await _driverDetailsRepository.GetByIdAsync(driverDetails.Id);
                if (freshDriverDetails is not null)
                {
                    freshDriverDetails.AssignVehicle(request.VehicleId);
                    await _driverDetailsRepository.UpdateAsync(freshDriverDetails);
                }
                
                return Result<DriverDto>.Success(_mapper.Map<DriverDto>(created));
            }
        }

        // If assigning a vehicle, validate it exists
        if (request.VehicleId.HasValue && request.VehicleId.Value != Guid.Empty)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId.Value);
            if (vehicle is null)
                return Result<DriverDto>.Failure("Vehicle not found.", ErrorType.NotFound);

            // Check if vehicle is already assigned to another driver
            var driverWithVehicle = await _driverRepository.GetByAssignedVehicleIdAsync(request.VehicleId.Value);
            if (driverWithVehicle is not null && driverWithVehicle.Id != driver.Id)
                throw new DomainException($"Vehicle is already assigned to another driver ({driverWithVehicle.Name}). A vehicle can only be assigned to one driver at a time.");
        }

        // Assign vehicle to Driver
        driver.AssignVehicle(request.VehicleId);

        // Update driver
        var updated = await _driverRepository.UpdateAsync(driver);
        
        // Get fresh DriverDetails and sync
        var freshDd = await _driverDetailsRepository.GetByUserIdAsync(driver.ApplicationUserId);
        
        if (freshDd is not null)
        {
            freshDd.AssignVehicle(request.VehicleId);
            await _driverDetailsRepository.UpdateAsync(freshDd);
        }

        return Result<DriverDto>.Success(_mapper.Map<DriverDto>(updated));
    }
}

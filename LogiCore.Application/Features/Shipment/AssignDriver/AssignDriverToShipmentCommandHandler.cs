using AutoMapper;
using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Features.Shipment.AssignDriver;

public class AssignDriverToShipmentCommandHandler : IRequestHandler<AssignDriverToShipmentCommand, Result<bool>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignDriverToShipmentCommandHandler(IShipmentRepository shipmentRepository, IDriverRepository driverRepository, IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(AssignDriverToShipmentCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (shipment == null)
            return Result<bool>.Failure("Shipment not found.");

        var driver = await _driverRepository.GetByIdAsync(request.DriverId);
        if (driver == null)
            return Result<bool>.Failure("Driver not found.");

        if (!driver.IsActive)
            return Result<bool>.Failure("Driver is not active.");

        try
        {
            shipment.AssignDriver(driver.Id);
            await _shipmentRepository.UpdateAsync(shipment);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (LogiCore.Domain.Common.Exceptions.DomainException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}

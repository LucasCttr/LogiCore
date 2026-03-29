using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using LogiCore.Application.Common.Interfaces.Persistence;

namespace LogiCore.Application.Features.Shipment.AssignDriver;

public class AssignDriverToShipmentCommandValidator : AbstractValidator<AssignDriverToShipmentCommand>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IShipmentRepository _shipmentRepository;

    public AssignDriverToShipmentCommandValidator(IDriverRepository driverRepository, IShipmentRepository shipmentRepository)
    {
        _driverRepository = driverRepository;
        _shipmentRepository = shipmentRepository;

        RuleFor(x => x.ShipmentId)
            .NotEmpty().WithMessage("ShipmentId is required.")
            .MustAsync(ShipmentExists).WithMessage("Shipment not found.");

        RuleFor(x => x.DriverId)
            .NotEmpty().WithMessage("DriverId is required.")
            .MustAsync(DriverExists).WithMessage("Driver not found or inactive.");
    }

    private async Task<bool> ShipmentExists(Guid shipmentId, CancellationToken ct)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(shipmentId);
        return shipment != null;
    }

    private async Task<bool> DriverExists(Guid driverId, CancellationToken ct)
    {
        var driver = await _driverRepository.GetByIdAsync(driverId);
        return driver != null && driver.IsActive;
    }
}

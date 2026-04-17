using FluentValidation;

namespace LogiCore.Application.Features.Shipment.StartShipment;

public class StartShipmentCommandValidator : AbstractValidator<StartShipmentCommand>
{
    public StartShipmentCommandValidator()
    {
        RuleFor(x => x.ShipmentId)
            .NotEmpty().WithMessage("ShipmentId is required.");
        
        // ScannedPackageIds is optional - no validation required
    }
}


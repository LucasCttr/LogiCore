using FluentValidation;

namespace LogiCore.Application.Features.Shipment.DispatchShipment;

public class DispatchShipmentCommandValidator : AbstractValidator<DispatchShipmentCommand>
{
    public DispatchShipmentCommandValidator()
    {
        RuleFor(x => x.ShipmentId)
            .NotEmpty().WithMessage("ShipmentId is required.");
    }
}

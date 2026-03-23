using FluentValidation;

namespace LogiCore.Application.Features.Shipment.CreateShipment;

public class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(x => x.RouteCode)
            .NotEmpty().WithMessage("RouteCode is required.")
            .MaximumLength(100);

        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("VehicleId is required.");
    }
}

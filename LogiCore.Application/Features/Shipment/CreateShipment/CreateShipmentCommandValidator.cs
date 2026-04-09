using FluentValidation;

namespace LogiCore.Application.Features.Shipment.CreateShipment;

public class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(x => x.DriverId)
            .NotEmpty().WithMessage("Driver is required.");

        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("Vehicle is required.");

        RuleFor(x => x.PackageIds)
            .NotEmpty().WithMessage("At least one package must be selected.")
            .Must(ids => ids.Count > 0).WithMessage("At least one package must be selected.");

        RuleFor(x => x.EstimatedDelivery)
            .NotEmpty().WithMessage("Estimated delivery date is required.")
            .GreaterThan(System.DateTime.UtcNow).WithMessage("Estimated delivery must be in the future.");
    }
}

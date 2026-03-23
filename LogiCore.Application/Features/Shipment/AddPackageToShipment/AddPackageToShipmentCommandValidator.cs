using FluentValidation;

namespace LogiCore.Application.Features.Shipment.AddPackageToShipment;

public class AddPackageToShipmentCommandValidator : AbstractValidator<AddPackageToShipmentCommand>
{
    public AddPackageToShipmentCommandValidator()
    {
        RuleFor(x => x.ShipmentId)
            .NotEmpty().WithMessage("ShipmentId is required.");

        RuleFor(x => x.PackageId)
            .NotEmpty().WithMessage("PackageId is required.");
    }
}


using FluentValidation;

namespace LogiCore.Application.Features.Packages;
public class ShipPackageCommandValidator : AbstractValidator<ShipPackageCommand>
{
    public ShipPackageCommandValidator()
    {
        RuleFor(x => x.PackageId)
            .NotEmpty().WithMessage("PackageId is required.");
    }
}
    

using FluentValidation;

namespace LogiCore.Application.Features.Packages;
public class DeliverPackageCommandValidator : AbstractValidator<DeliverPackageCommand>
{
    public DeliverPackageCommandValidator()
    {
        RuleFor(x => x.PackageId)
            .NotEmpty().WithMessage("PackageId is required.");
    }
}
    
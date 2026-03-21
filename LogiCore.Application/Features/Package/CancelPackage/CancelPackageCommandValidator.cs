using FluentValidation;

namespace LogiCore.Application.Features.Packages;

public class CancelPackageCommandValidator : AbstractValidator<CancelPackageCommand>
{
    public CancelPackageCommandValidator()
    {
        RuleFor(x => x.PackageId)
            .NotEmpty().WithMessage("PackageId is required.");
    }
}

using FluentValidation;

namespace LogiCore.Application.Features.Packages;
public class CreatePackageCommandValidator : AbstractValidator<CreatePackageCommand>
{
    public CreatePackageCommandValidator()
    {
        RuleFor(x => x.TrackingNumber)
            .NotEmpty().WithMessage("TrackingNumber is required.")
            .MaximumLength(50);

        RuleFor(x => x.RecipientName)
            .NotEmpty().WithMessage("RecipientName is required.")
            .MaximumLength(200);

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than zero.");
    }
}

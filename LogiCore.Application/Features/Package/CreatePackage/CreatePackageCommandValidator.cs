
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

        RuleFor(x => x.RecipientCity)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.RecipientCity));

        RuleFor(x => x.RecipientPostalCode)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.RecipientPostalCode));

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than zero.");

        RuleFor(x => x.LengthCm)
            .GreaterThan(0).WithMessage("Length must be greater than zero.");

        RuleFor(x => x.WidthCm)
            .GreaterThan(0).WithMessage("Width must be greater than zero.");

        RuleFor(x => x.HeightCm)
            .GreaterThan(0).WithMessage("Height must be greater than zero.");
    }
}

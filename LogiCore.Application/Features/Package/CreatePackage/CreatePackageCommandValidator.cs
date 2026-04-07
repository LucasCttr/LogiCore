
using FluentValidation;

namespace LogiCore.Application.Features.Packages;
public class CreatePackageCommandValidator : AbstractValidator<CreatePackageCommand>
{
    public CreatePackageCommandValidator()
    {
        RuleFor(x => x.TrackingNumber)
            .NotEmpty().WithMessage("TrackingNumber is required.")
            .MaximumLength(50);
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.InternalCode)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.InternalCode));
        // Recipient fields are optional for the simplified frontend flow. If provided, apply basic constraints.
        RuleFor(x => x.RecipientName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.RecipientName));

        RuleFor(x => x.RecipientCity)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.RecipientCity));

        RuleFor(x => x.RecipientPostalCode)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.RecipientPostalCode));

        // Weight is optional but if present must be > 0
        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than zero.")
            .When(x => x.Weight.HasValue);

        // Dimensions optional
        RuleFor(x => x.LengthCm)
            .GreaterThan(0).WithMessage("Length must be greater than zero.")
            .When(x => x.LengthCm.HasValue);

        RuleFor(x => x.WidthCm)
            .GreaterThan(0).WithMessage("Width must be greater than zero.")
            .When(x => x.WidthCm.HasValue);

        RuleFor(x => x.HeightCm)
            .GreaterThan(0).WithMessage("Height must be greater than zero.")
            .When(x => x.HeightCm.HasValue);
    }
}

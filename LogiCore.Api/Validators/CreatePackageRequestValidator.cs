namespace LogiCore.Api.Validators;
using FluentValidation;
using LogiCore.Api.Models.DTOs;

public class CreatePackageRequestValidator : AbstractValidator<CreatePackageRequest>
{
    public CreatePackageRequestValidator()
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
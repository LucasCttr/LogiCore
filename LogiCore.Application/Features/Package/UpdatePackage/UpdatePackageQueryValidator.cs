using FluentValidation;

namespace LogiCore.Application.Features.Packages;
public class UpdatePackagesQueryValidator : AbstractValidator<UpdatePackageCommand>
{
    public UpdatePackagesQueryValidator()
    {
        // Only validate weight when it's provided (partial update)
        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than zero.")
            .When(x => x.Weight.HasValue);
    }
}

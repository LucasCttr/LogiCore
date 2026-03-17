using FluentValidation;

namespace LogiCore.Application.Features.Packages;
public class GetAllPackagesQueryValidator : AbstractValidator<GetAllPackagesQuery>
{
    private const int MaxPageSize = 100;

    public GetAllPackagesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(MaxPageSize);
    }
}

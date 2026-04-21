using MediatR;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Package.MarkAttemptFailed;

public record MarkPackageAttemptFailedCommand : IRequest<Result<bool>>
{
    public Guid PackageId { get; init; }
    public string? Reason { get; init; }
}

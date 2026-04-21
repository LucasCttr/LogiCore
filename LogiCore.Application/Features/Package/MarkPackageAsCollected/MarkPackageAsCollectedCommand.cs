using MediatR;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Package.MarkPackageAsCollected;

public record MarkPackageAsCollectedCommand : IRequest<Result<bool>>
{
    public Guid PackageId { get; init; }
    public string? CollectionNotes { get; init; }
}

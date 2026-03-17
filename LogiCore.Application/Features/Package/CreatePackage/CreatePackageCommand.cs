using MediatR;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Package; 
public class CreatePackageCommand : IRequest<Result<Guid>>
{
    public required string TrackingNumber { get; init; }
    public required string RecipientName { get; init; }
    public decimal Weight { get; init; }
}
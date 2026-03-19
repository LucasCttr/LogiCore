using System.Threading;
using System.Threading.Tasks;
using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;

namespace LogiCore.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that commits the UnitOfWork after a successful request handler.
/// </summary>
public class SaveChangesBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public SaveChangesBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        // Commit changes if any. Handlers that already call SaveChangesAsync will still work;
        // this ensures a single commit per request when handlers don't.
        await _unitOfWork.CommitAsync(cancellationToken);

        return response;
    }
}

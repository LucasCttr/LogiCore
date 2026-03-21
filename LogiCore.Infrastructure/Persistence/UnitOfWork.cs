using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogiCore.Application.Common.Interfaces.Persistence;
using MediatR;
using LogiCore.Domain.Common;

namespace LogiCore.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly LogiCoreDbContext _dbContext;
    private readonly IMediator _mediator;

    public UnitOfWork(LogiCoreDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return CommitWithDomainEventsAsync(cancellationToken);
    }

    private async Task<int> CommitWithDomainEventsAsync(CancellationToken cancellationToken)
    {
        // Identify entities with domain events
        var domainEntities = _dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        // Extract all domain events from those entities
        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        // Clear domain events from entities to prevent re-processing in case of retries or multiple handlers
        foreach (var entity in domainEntities)
        {
            entity.ClearDomainEvents();
        }

        // Save changes to the database first. If this fails, we won't publish any events.
        var result = await _dbContext.SaveChangesAsync(cancellationToken);

        // Publish domain events after successful commit. This ensures that events are only published if the transaction succeeds.
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}

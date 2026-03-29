using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogiCore.Application.Common.Interfaces.Persistence;
using MediatR;
using LogiCore.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage;
using System;

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

    private class EfUnitOfWorkTransaction : IUnitOfWorkTransaction
    {
        private readonly IDbContextTransaction _tx;

        public EfUnitOfWorkTransaction(IDbContextTransaction tx) => _tx = tx;

        public Task CommitAsync(CancellationToken cancellationToken = default) => _tx.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken = default) => _tx.RollbackAsync(cancellationToken);

        public ValueTask DisposeAsync() => _tx.DisposeAsync();
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return CommitWithDomainEventsAsync(cancellationToken);
    }

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new EfUnitOfWorkTransaction(tx);
    }

    private async Task<int> CommitWithDomainEventsAsync(CancellationToken cancellationToken)
    {
        // Process domain events in a loop so handlers that publish new events are handled too.
        while (true)
        {
            var domainEntries = _dbContext.ChangeTracker
                .Entries<IHasDomainEvents>()
                .Where(x => x.Entity.DomainEvents.Any())
                .ToList();

            if (!domainEntries.Any())
                break;

            var domainEvents = domainEntries
                .SelectMany(e => e.Entity.DomainEvents)
                .ToList();

            // Clear events before publishing to avoid re-processing on retries
            foreach (var entry in domainEntries)
            {
                entry.Entity.ClearDomainEvents();
            }

            // Publish all collected events. Handlers may add entities or domain events to the DbContext.
            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
        }

        // Single save at the end: persist aggregate changes and any additions made by event handlers together.
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

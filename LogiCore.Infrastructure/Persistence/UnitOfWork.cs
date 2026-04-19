using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogiCore.Application.Common.Interfaces.Persistence;
using MediatR;
using LogiCore.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;

namespace LogiCore.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly LogiCoreDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(LogiCoreDbContext dbContext, IMediator mediator, ILogger<UnitOfWork> logger)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _logger = logger;
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
        _logger.LogInformation($"[UnitOfWork.CommitWithDomainEventsAsync] Starting commit. ChangeTracker has {_dbContext.ChangeTracker.Entries().Count()} entries");
        
        // Process domain events in a loop so handlers that publish new events are handled too.
        int eventLoopIteration = 0;
        while (true)
        {
            eventLoopIteration++;
            _logger.LogInformation($"[UnitOfWork.CommitWithDomainEventsAsync] Event loop iteration {eventLoopIteration}");
            
            var domainEntries = _dbContext.ChangeTracker
                .Entries<IHasDomainEvents>()
                .Where(x => x.Entity.DomainEvents.Any())
                .ToList();

            if (!domainEntries.Any())
            {
                _logger.LogInformation($"[UnitOfWork.CommitWithDomainEventsAsync] No domain events found. Breaking loop.");
                break;
            }

            var domainEvents = domainEntries
                .SelectMany(e => e.Entity.DomainEvents)
                .ToList();
            
            _logger.LogInformation($"[UnitOfWork.CommitWithDomainEventsAsync] Found {domainEvents.Count} domain events from {domainEntries.Count} entities");

            // Clear events before publishing to avoid re-processing on retries
            foreach (var entry in domainEntries)
            {
                entry.Entity.ClearDomainEvents();
            }

            // Publish all collected events. Handlers may add entities or domain events to the DbContext.
            foreach (var domainEvent in domainEvents)
            {
                _logger.LogInformation($"[UnitOfWork.CommitWithDomainEventsAsync] Publishing event: {domainEvent.GetType().Name}");
                await _mediator.Publish(domainEvent, cancellationToken);
            }
        }

        // Single save at the end: persist aggregate changes and any additions made by event handlers together.
        _logger.LogInformation($"[UnitOfWork.CommitWithDomainEventsAsync] Calling SaveChangesAsync()");
        var rowsAffected = await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation($"[UnitOfWork.CommitWithDomainEventsAsync] SaveChangesAsync completed. Rows affected: {rowsAffected}");
        return rowsAffected;
    }
}

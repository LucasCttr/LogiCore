using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using LogiCore.Domain.Common.Events;
using LogiCore.Infrastructure.Persistence;
using LogiCore.Domain.Entities;

namespace LogiCore.Infrastructure.Events;

internal class PackageStatusChangedHandler : INotificationHandler<PackageStatusChangedEvent>
{
    private readonly LogiCoreDbContext _dbContext;

    public PackageStatusChangedHandler(LogiCoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task Handle(PackageStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        var history = new PackageStatusHistory
        {
            Id = Guid.NewGuid(),
            PackageId = notification.PackageId,
            FromStatus = notification.OldStatus,
            ToStatus = notification.NewStatus,
            OccurredAt = notification.OccurredOn
        };

        _dbContext.Add(history);

        // Do not SaveChanges here; Save will be executed by UnitOfWork after handlers run
        return Task.CompletedTask;
    }
}

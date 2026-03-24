using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using LogiCore.Domain.Common.Events;
using LogiCore.Infrastructure.Persistence;
using LogiCore.Application.Common.Interfaces.Security;
using LogiCore.Domain.Entities;

namespace LogiCore.Infrastructure.Events;

internal class PackageStatusChangedHandler : INotificationHandler<PackageStatusChangedEvent>
{
    private readonly LogiCoreDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public PackageStatusChangedHandler(LogiCoreDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public Task Handle(PackageStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        var history = new PackageStatusHistory
        {
            Id = Guid.NewGuid(),
            PackageId = notification.PackageId,
            FromStatus = notification.OldStatus,
            ToStatus = notification.NewStatus,
            OccurredAt = notification.OccurredOn,
            EmployeeId = _currentUserService?.UserId,
            InternalNotes = null
        };

        _dbContext.Add(history);

        // Do not SaveChanges here; Save will be executed by UnitOfWork after handlers run
        return Task.CompletedTask;
    }
}

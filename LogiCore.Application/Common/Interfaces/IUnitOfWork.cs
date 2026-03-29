using System.Threading;
using System.Threading.Tasks;

namespace LogiCore.Application.Common.Interfaces.Persistence;

public interface IUnitOfWork
{
    /// <summary>
    /// Commits changes to the underlying persistence store.
    /// </summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a logical transaction scope that can be committed or rolled back.
    /// Returns an abstraction to avoid leaking EF Core types to the application layer.
    /// </summary>
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

public interface IUnitOfWorkTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

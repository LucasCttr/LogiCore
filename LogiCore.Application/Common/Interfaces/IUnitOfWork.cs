using System.Threading;
using System.Threading.Tasks;

namespace LogiCore.Application.Common.Interfaces.Persistence;

public interface IUnitOfWork
{
    /// <summary>
    /// Commits changes to the underlying persistence store.
    /// </summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}

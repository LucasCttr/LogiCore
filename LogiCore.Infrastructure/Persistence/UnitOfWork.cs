using System.Threading;
using System.Threading.Tasks;
using LogiCore.Application.Common.Interfaces.Persistence;

namespace LogiCore.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly LogiCoreDbContext _dbContext;

    public UnitOfWork(LogiCoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

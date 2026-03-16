using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Domain.Entities;
using LogiCore.Infrastructure.Persistence;

public class SqlPackageRepository : IPackageRepository
{
    private readonly LogiCoreDbContext _context; 

    public SqlPackageRepository(LogiCoreDbContext context)
    {
        _context = context;
    }
    public Task AddAsync(Package package)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Package>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Package?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync()
    {
        throw new NotImplementedException();
    }
}
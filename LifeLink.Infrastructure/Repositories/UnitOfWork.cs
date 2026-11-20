using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LifeLink.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly LifeLinkDbContext _context;

    public UnitOfWork(LifeLinkDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            await operation(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
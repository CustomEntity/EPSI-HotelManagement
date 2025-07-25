using HotelManagement.Domain.Common;

namespace HotelManagement.Infrastructure.Persistence;

public class InMemoryUnitOfWork : IUnitOfWork
{
    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public void Dispose()
    {
    }
}
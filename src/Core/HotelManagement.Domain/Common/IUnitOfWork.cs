using System.Threading;
using System.Threading.Tasks;

namespace HotelManagement.Domain.Common;

public interface IUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
} 
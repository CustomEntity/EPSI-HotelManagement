using HotelManagement.Domain.Common;
using HotelManagement.Domain.Customer.Aggregates;
using HotelManagement.Domain.Customer.ValueObjects;

namespace HotelManagement.Domain.Customer;

public interface ICustomerRepository : IRepository<CustomerAggregate, CustomerId>
{
    Task<CustomerAggregate?> GetByIdAsync(CustomerId customerId, CancellationToken cancellationToken = default);
    Task<CustomerAggregate?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerAggregate>> GetByTypeAsync(CustomerType customerType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerAggregate>> GetActiveCustomersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerAggregate>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(CustomerId customerId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default);
    Task<int> GetTotalActiveCustomersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerAggregate>> GetVIPCustomersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerAggregate>> GetCustomersCreatedAfterAsync(DateTime date, CancellationToken cancellationToken = default);
}
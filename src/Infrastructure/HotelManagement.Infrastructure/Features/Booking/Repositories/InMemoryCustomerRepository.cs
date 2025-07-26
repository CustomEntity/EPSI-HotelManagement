using HotelManagement.Domain.Customer;
using HotelManagement.Domain.Customer.Aggregates;
using HotelManagement.Domain.Customer.ValueObjects;

namespace HotelManagement.Infrastructure.Features.Booking.Repositories;

public class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly List<CustomerAggregate> _customers = new();

    public InMemoryCustomerRepository()
    {
        SeedTestData();
    }

    public Task<CustomerAggregate?> GetByIdAsync(CustomerId id, CancellationToken cancellationToken = default)
    {
        var customer = _customers.FirstOrDefault(c => c.Id.Value == id.Value);
        return Task.FromResult(customer);
    }

    public Task<CustomerAggregate?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var customer = _customers.FirstOrDefault(c => c.Email == email);
        return Task.FromResult(customer);
    }

    public Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default)
    {
        var exists = _customers.Any(c => c.Email == email);
        return Task.FromResult(exists);
    }

    public Task<bool> ExistsAsync(CustomerId id, CancellationToken cancellationToken = default)
    {
        var exists = _customers.Any(c => c.Id == id);
        return Task.FromResult(exists);
    }

    public Task<IReadOnlyList<CustomerAggregate>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
    {
        var activeCustomers = _customers
            .Where(c => c.IsActive)
            .ToList();
        return Task.FromResult<IReadOnlyList<CustomerAggregate>>(activeCustomers);
    }

    public Task<IReadOnlyList<CustomerAggregate>> GetByTypeAsync(CustomerType customerType, CancellationToken cancellationToken = default)
    {
        var customers = _customers
            .Where(c => c.CustomerType == customerType)
            .ToList();
        return Task.FromResult<IReadOnlyList<CustomerAggregate>>(customers);
    }

    public Task<IReadOnlyList<CustomerAggregate>> GetCustomersCreatedAfterAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var customers = _customers
            .Where(c => c.CreatedAt > date)
            .ToList();
        return Task.FromResult<IReadOnlyList<CustomerAggregate>>(customers);
    }

    public Task<int> GetTotalActiveCustomersAsync(CancellationToken cancellationToken = default)
    {
        var count = _customers.Count(c => c.IsActive);
        return Task.FromResult(count);
    }

    public Task<IReadOnlyList<CustomerAggregate>> GetVIPCustomersAsync(CancellationToken cancellationToken = default)
    {
        var vipCustomers = _customers
            .Where(c => c.CustomerType.IsVIP)
            .ToList();
        return Task.FromResult<IReadOnlyList<CustomerAggregate>>(vipCustomers);
    }

    public Task<IReadOnlyList<CustomerAggregate>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Task.FromResult<IReadOnlyList<CustomerAggregate>>(new List<CustomerAggregate>());
        }

        var searchTermLower = searchTerm.ToLowerInvariant();
        var customers = _customers
            .Where(c => 
                c.Name.FirstName.ToLowerInvariant().Contains(searchTermLower) ||
                c.Name.LastName.ToLowerInvariant().Contains(searchTermLower) ||
                c.Name.FullName.ToLowerInvariant().Contains(searchTermLower))
            .ToList();
        
        return Task.FromResult<IReadOnlyList<CustomerAggregate>>(customers);
    }

    public Task<List<CustomerAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_customers.ToList());
    }

    public Task AddAsync(CustomerAggregate entity, CancellationToken cancellationToken = default)
    {
        _customers.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(CustomerAggregate entity, CancellationToken cancellationToken = default)
    {
        var existingCustomer = _customers.FirstOrDefault(c => c.Id.Value == entity.Id.Value);
        if (existingCustomer != null)
        {
            _customers.Remove(existingCustomer);
            _customers.Add(entity);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(CustomerAggregate entity, CancellationToken cancellationToken = default)
    {
        var customer = _customers.FirstOrDefault(c => c.Id.Value == entity.Id.Value);
        if (customer != null)
        {
            _customers.Remove(customer);
        }
        return Task.CompletedTask;
    }

    private void SeedTestData()
    {
        try
        {
            var customer1 = CreateTestCustomer("john.doe@example.com", "John", "Doe", "+33123456789", CustomerType.Individual);
            var customer2 = CreateTestCustomer("jane.smith@example.com", "Jane", "Smith", "+33987654321", CustomerType.VIP);
            var customer3 = CreateTestCustomer("bob.wilson@example.com", "Bob", "Wilson", "+33555666777", CustomerType.Corporate);
            var customer4 = CreateTestCustomer("alice.brown@example.com", "Alice", "Brown", "+33444555666", CustomerType.VIP);

            if (customer1 != null) _customers.Add(customer1);
            if (customer2 != null) _customers.Add(customer2);
            if (customer3 != null) _customers.Add(customer3);
            if (customer4 != null) _customers.Add(customer4);
        }
        catch
        {
            // Ignore seeding errors
        }
    }

    private CustomerAggregate? CreateTestCustomer(string email, string firstName, string lastName, string phone, CustomerType customerType)
    {
        try
        {
            var customerEmail = Email.Create(email);
            var customerName = PersonalName.Create(firstName, lastName);
            var customerPhone = PhoneNumber.Create(phone);

            var customerResult = CustomerAggregate.Create(
                customerName,
                customerEmail,
                customerType,
                customerPhone);

            return customerResult.IsSuccess ? customerResult.Value : null;
        }
        catch
        {
            return null;
        }
    }
}
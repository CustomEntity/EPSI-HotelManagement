using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Customer.ValueObjects;
using HotelManagement.Domain.Payment;
using HotelManagement.Domain.Payment.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Infrastructure.Features.Booking.Repositories;

public class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly List<Domain.Payment.Aggregates.Payment> _payments = new();

    public InMemoryPaymentRepository()
    {
        SeedTestData();
    }

    public Task<Domain.Payment.Aggregates.Payment?> GetByIdAsync(PaymentId id, CancellationToken cancellationToken = default)
    {
        var payment = _payments.FirstOrDefault(p => p.Id.Value == id.Value);
        return Task.FromResult(payment);
    }

    public Task<Domain.Payment.Aggregates.Payment?> GetByBookingIdAsync(BookingId bookingId, CancellationToken cancellationToken = default)
    {
        var payment = _payments.FirstOrDefault(p => p.BookingId.Value == bookingId.Value);
        return Task.FromResult(payment);
    }

    public Task<List<Domain.Payment.Aggregates.Payment>> GetByCustomerIdAsync(CustomerId customerId,
        CancellationToken cancellationToken = default)
    {
        var payments = _payments
            .Where(p => p.CustomerId.Value == customerId.Value)
            .ToList();
        return Task.FromResult(payments);
    }

    public Task<List<Domain.Payment.Aggregates.Payment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_payments.ToList());
    }

    public Task<List<Domain.Payment.Aggregates.Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        var payments = _payments
            .Where(p => p.Status == status)
            .ToList();
        return Task.FromResult(payments);
    }

    public Task<List<Domain.Payment.Aggregates.Payment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var payments = _payments
            .Where(p => p.CreatedAt >= fromDate && p.CreatedAt <= toDate)
            .ToList();
        return Task.FromResult(payments);
    }

    // Nouvelles méthodes de l'interface

    public Task<Domain.Payment.Aggregates.Payment?> GetByIdWithRefundsAsync(PaymentId id, CancellationToken cancellationToken = default)
    {
        var payment = _payments.FirstOrDefault(p => p.Id.Value == id.Value);
        return Task.FromResult(payment);
    }

    public Task<List<Domain.Payment.Aggregates.Payment>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var payments = _payments
            .Where(p => p.CreatedAt.Date >= fromDate.Date && p.CreatedAt.Date <= toDate.Date)
            .OrderBy(p => p.CreatedAt)
            .ToList();
        return Task.FromResult(payments);
    }

    public Task<List<Domain.Payment.Aggregates.Payment>> GetPaymentsWithPendingRefundsAsync(CancellationToken cancellationToken = default)
    {
        var payments = _payments
            .Where(p => p.HasPendingRefunds())
            .ToList();
        return Task.FromResult(payments);
    }

    public Task<decimal> GetTotalRefundedAsync(DateTime? fromDate = null, DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _payments.AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= toDate.Value);
        }

        var totalRefunded = query
            .Where(p => p.IsFullyRefunded())
            .Sum(p => p.GetTotalRefundedAmount().Amount);

        return Task.FromResult(totalRefunded);
    }

    public Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _payments.AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(p => p.ProcessedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(p => p.ProcessedAt <= toDate.Value);
        }

        var totalRevenue = query
            .Where(p => p.Status.IsSuccessful())
            .Sum(p => p.GetRemainingAmount().Amount);

        return Task.FromResult(totalRevenue);
    }

    // Méthodes CRUD

    public Task AddAsync(Domain.Payment.Aggregates.Payment entity, CancellationToken cancellationToken = default)
    {
        _payments.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Domain.Payment.Aggregates.Payment entity, CancellationToken cancellationToken = default)
    {
        var existingPayment = _payments.FirstOrDefault(p => p.Id.Value == entity.Id.Value);
        if (existingPayment != null)
        {
            _payments.Remove(existingPayment);
            _payments.Add(entity);
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Domain.Payment.Aggregates.Payment entity, CancellationToken cancellationToken = default)
    {
        var payment = _payments.FirstOrDefault(p => p.Id.Value == entity.Id.Value);
        if (payment != null)
        {
            _payments.Remove(payment);
        }

        return Task.CompletedTask;
    }

    private void SeedTestData()
    {
        try
        {
            var payment1 = CreateTestPayment(
                new Money(250.00m, Currency.EUR),
                PaymentMethod.CreditCard);

            var payment2 = CreateTestPayment(
                new Money(180.00m, Currency.EUR),
                PaymentMethod.PayPal);

            var payment3 = CreateTestPayment(
                new Money(320.00m, Currency.EUR),
                PaymentMethod.BankTransfer);

            if (payment1 != null) _payments.Add(payment1);
            if (payment2 != null) _payments.Add(payment2);
            if (payment3 != null) _payments.Add(payment3);
        }
        catch
        {
            // Ignore seeding errors
        }
    }

    private Domain.Payment.Aggregates.Payment? CreateTestPayment(Money amount, PaymentMethod method)
    {
        try
        {
            var bookingId = BookingId.Create(Guid.NewGuid());
            var customerId = CustomerId.Create(Guid.NewGuid());

            var paymentResult = Domain.Payment.Aggregates.Payment.Create(
                bookingId,
                customerId,
                amount,
                method);

            return paymentResult.IsSuccess ? paymentResult.Value : null;
        }
        catch
        {
            return null;
        }
    }
}
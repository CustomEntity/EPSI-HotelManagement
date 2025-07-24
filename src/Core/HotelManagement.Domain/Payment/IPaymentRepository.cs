using HotelManagement.Domain.Common;
using HotelManagement.Domain.Payment.Aggregates;
using HotelManagement.Domain.Payment.ValueObjects;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Customer.ValueObjects;

namespace HotelManagement.Domain.Payment;

public interface IPaymentRepository : IRepository<Aggregates.Payment, PaymentId>
{
    Task<Aggregates.Payment?> GetByIdWithRefundsAsync(PaymentId paymentId, CancellationToken cancellationToken = default);
    Task<Aggregates.Payment?> GetByBookingIdAsync(BookingId bookingId, CancellationToken cancellationToken = default);
    Task<List<Aggregates.Payment>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default);
    Task<List<Aggregates.Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    Task<List<Aggregates.Payment>> GetPaymentsWithPendingRefundsAsync(CancellationToken cancellationToken = default);
    Task<List<Aggregates.Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRefundedAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}
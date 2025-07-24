using HotelManagement.Domain.Payment.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Payment.Services;

public interface IPaymentGateway
{
    Task<Result<TransactionReference>> ProcessPaymentAsync(
        Money amount,
        CreditCard creditCard,
        string description,
        CancellationToken cancellationToken = default);

    Task<Result<TransactionReference>> ProcessRefundAsync(
        TransactionReference originalTransaction,
        Money amount,
        string reason,
        CancellationToken cancellationToken = default);

    Task<Result<PaymentStatus>> GetPaymentStatusAsync(
        TransactionReference transactionReference,
        CancellationToken cancellationToken = default);
}
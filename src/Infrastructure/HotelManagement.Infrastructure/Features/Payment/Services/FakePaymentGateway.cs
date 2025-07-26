// Infrastructure/Services/MockPaymentGateway.cs

using HotelManagement.Domain.Payment.Services;
using HotelManagement.Domain.Payment.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Infrastructure.Features.Payment.Services;

public class FakePaymentGateway: IPaymentGateway
{
    public Task<Result<TransactionReference>> ProcessPaymentAsync(
        Money amount,
        CreditCard creditCard,
        string description,
        CancellationToken cancellationToken = default)
    {
        // Simulation d'un traitement de paiement réussi
        var transactionRef = TransactionReference.Create(
            $"TXN-{Guid.NewGuid():N}",
            "MockGateway");

        return Task.FromResult(Result<TransactionReference>.Success(transactionRef));
    }

    public Task<Result<TransactionReference>> ProcessRefundAsync(
        TransactionReference originalTransaction,
        Money amount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        // Simulation d'un remboursement réussi
        var refundRef = TransactionReference.Create(
            $"REF-{Guid.NewGuid():N}",
            "MockGateway");

        return Task.FromResult(Result<TransactionReference>.Success(refundRef));
    }

    public Task<Result<PaymentStatus>> GetPaymentStatusAsync(
        TransactionReference transactionReference,
        CancellationToken cancellationToken = default)
    {
        // Simulation - retourne toujours "Completed" pour les tests
        return Task.FromResult(Result<PaymentStatus>.Success(PaymentStatus.Completed));
    }
}
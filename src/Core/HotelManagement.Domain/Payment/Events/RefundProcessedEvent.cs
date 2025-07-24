using HotelManagement.Domain.Common;
using HotelManagement.Domain.Payment.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Payment.Events;

public sealed class RefundProcessedEvent : DomainEventBase
{
    public PaymentId PaymentId { get; }
    public Guid RefundId { get; }
    public Money Amount { get; }
    public TransactionReference TransactionReference { get; }

    public RefundProcessedEvent(
        PaymentId paymentId, 
        Guid refundId, 
        Money amount,
        TransactionReference transactionReference)
    {
        PaymentId = paymentId;
        RefundId = refundId;
        Amount = amount;
        TransactionReference = transactionReference;
    }
}
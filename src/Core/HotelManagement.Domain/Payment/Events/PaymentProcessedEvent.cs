using HotelManagement.Domain.Common;
using HotelManagement.Domain.Payment.ValueObjects;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Payment.Events;

public sealed class PaymentProcessedEvent : DomainEventBase
{
    public PaymentId PaymentId { get; }
    public BookingId BookingId { get; }
    public Money Amount { get; }
    public PaymentMethod PaymentMethod { get; }
    public TransactionReference TransactionReference { get; }

    public PaymentProcessedEvent(
        PaymentId paymentId, 
        BookingId bookingId, 
        Money amount,
        PaymentMethod paymentMethod,
        TransactionReference transactionReference)
    {
        PaymentId = paymentId;
        BookingId = bookingId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        TransactionReference = transactionReference;
    }
}
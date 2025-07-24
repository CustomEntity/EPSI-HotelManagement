using HotelManagement.Domain.Common;
using HotelManagement.Domain.Payment.ValueObjects;
using HotelManagement.Domain.Booking.ValueObjects;

namespace HotelManagement.Domain.Payment.Events;

public sealed class PaymentFailedEvent : DomainEventBase
{
    public PaymentId PaymentId { get; }
    public BookingId BookingId { get; }
    public string FailureReason { get; }

    public PaymentFailedEvent(PaymentId paymentId, BookingId bookingId, string failureReason)
    {
        PaymentId = paymentId;
        BookingId = bookingId;
        FailureReason = failureReason;
    }
}
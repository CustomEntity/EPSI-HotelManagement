using HotelManagement.Domain.Common;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Payment.ValueObjects;

namespace HotelManagement.Domain.Booking.Events;

public sealed class BookingConfirmedEvent : DomainEventBase
{
    public BookingId BookingId { get; }
    public PaymentId PaymentId { get; }

    public BookingConfirmedEvent(BookingId bookingId, PaymentId paymentId)
    {
        BookingId = bookingId;
        PaymentId = paymentId;
    }
}
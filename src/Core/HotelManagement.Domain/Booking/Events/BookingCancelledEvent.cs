using HotelManagement.Domain.Common;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Payment.ValueObjects;

namespace HotelManagement.Domain.Booking.Events;

public sealed class BookingCancelledEvent : DomainEventBase
{
    public BookingId BookingId { get; }
    public bool IsRefundable { get; }
    public PaymentId? PaymentId { get; }
    public string Reason { get; }
    public decimal RefundPercentage { get; }

    public BookingCancelledEvent(
        BookingId bookingId, 
        bool isRefundable, 
        PaymentId? paymentId, 
        string reason,
        decimal refundPercentage)
    {
        BookingId = bookingId;
        IsRefundable = isRefundable;
        PaymentId = paymentId;
        Reason = reason;
        RefundPercentage = refundPercentage;
    }
}
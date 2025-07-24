using HotelManagement.Domain.Common;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Customer.ValueObjects;

namespace HotelManagement.Domain.Booking.Events;

public sealed class BookingCreatedEvent : DomainEventBase
{
    public BookingId BookingId { get; }
    public CustomerId CustomerId { get; }
    public DateRange DateRange { get; }
    public int RoomCount { get; }

    public BookingCreatedEvent(BookingId bookingId, CustomerId customerId, DateRange dateRange, int roomCount)
    {
        BookingId = bookingId;
        CustomerId = customerId;
        DateRange = dateRange;
        RoomCount = roomCount;
    }
}
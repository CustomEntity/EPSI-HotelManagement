using HotelManagement.Domain.Booking.Aggregates;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Booking.Services;

public interface IBookingDomainService
{
    Task<bool> IsRoomAvailableAsync(RoomId roomId, DateRange dateRange, CancellationToken cancellationToken = default);
    Task<Result<Money>> CalculateBookingCostAsync(BookingRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> ValidateBookingRulesAsync(BookingAggregate booking, CancellationToken cancellationToken = default);
}

public class BookingRequest
{
    public CustomerId CustomerId { get; }
    public List<RoomBookingInfo> Rooms { get; }
    public DateRange DateRange { get; }

    public BookingRequest(CustomerId customerId, List<RoomBookingInfo> rooms, DateRange dateRange)
    {
        CustomerId = customerId;
        Rooms = rooms;
        DateRange = dateRange;
    }
}

public class RoomBookingInfo
{
    public RoomId RoomId { get; }
    public int NumberOfGuests { get; }

    public RoomBookingInfo(RoomId roomId, int numberOfGuests)
    {
        RoomId = roomId;
        NumberOfGuests = numberOfGuests;
    }
} 
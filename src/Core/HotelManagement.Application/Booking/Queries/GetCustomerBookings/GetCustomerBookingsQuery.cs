using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Booking.Queries.GetCustomerBookings;

public sealed class GetCustomerBookingsQuery : IRequest<Result<List<CustomerBookingDto>>>
{
    public Guid CustomerId { get; init; }
    public string? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public sealed class CustomerBookingDto
{
    public Guid BookingId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Status { get; init; } = null!;
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = null!;
    public decimal FinalAmount { get; init; }
    public string CancellationPolicy { get; init; } = null!;
    public DateTime? ConfirmedAt { get; init; }
    public DateTime? CancelledAt { get; init; }
    public string? CancellationReason { get; init; }
    public DateTime? CheckInTime { get; init; }
    public DateTime? CheckOutTime { get; init; }
    public DateTime CreatedAt { get; init; }
    public int NumberOfRooms { get; init; }
    public int NumberOfNights { get; init; }
    public List<BookingRoomSummaryDto> Rooms { get; init; } = new();
}

public sealed class BookingRoomSummaryDto
{
    public Guid RoomId { get; init; }
    public string RoomNumber { get; init; } = null!;
    public string RoomType { get; init; } = null!;
    public decimal PricePerNight { get; init; }
    public string Currency { get; init; } = null!;
}
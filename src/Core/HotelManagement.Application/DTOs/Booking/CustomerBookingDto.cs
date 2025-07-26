namespace HotelManagement.Application.DTOs.Booking;

public class CustomerBookingDto
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
    public int GuestCount { get; init; }
}

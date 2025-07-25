using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Booking.Queries.GetBookingDetails;

public sealed class GetBookingDetailsQuery : IRequest<Result<BookingDetailsDto>>
{
    public Guid BookingId { get; init; }
}

public sealed class BookingDetailsDto
{
    public Guid BookingId { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Status { get; init; } = null!;
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = null!;
    public decimal? DiscountAmount { get; init; }
    public decimal FinalAmount { get; init; }
    public Guid? PaymentId { get; init; }
    public string CancellationPolicy { get; init; } = null!;
    public DateTime? ConfirmedAt { get; init; }
    public DateTime? CancelledAt { get; init; }
    public string? CancellationReason { get; init; }
    public DateTime? CheckInTime { get; init; }
    public DateTime? CheckOutTime { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public List<BookingItemDto> Items { get; init; } = new();
}

public sealed class BookingItemDto
{
    public Guid BookingItemId { get; init; }
    public Guid RoomId { get; init; }
    public string RoomNumber { get; init; } = null!;
    public string RoomType { get; init; } = null!;
    public decimal PricePerNight { get; init; }
    public string Currency { get; init; } = null!;
    public int NumberOfNights { get; init; }
    public decimal SubTotal { get; init; }
}
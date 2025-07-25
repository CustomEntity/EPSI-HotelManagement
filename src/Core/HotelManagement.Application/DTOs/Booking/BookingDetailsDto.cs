using System;
using System.Collections.Generic;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Application.DTOs.Booking;

public class BookingDetailsDto
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
    public int GuestCount { get; init; }
}

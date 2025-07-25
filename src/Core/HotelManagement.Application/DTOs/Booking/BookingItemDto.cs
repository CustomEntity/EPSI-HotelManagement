using System;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Application.DTOs.Booking;

public class BookingItemDto
{
    public Guid BookingItemId { get; init; }
    public Guid RoomId { get; init; }
    public string RoomNumber { get; init; } = null!;
    public string RoomType { get; init; } = null!;
    public decimal PricePerNight { get; init; }
    public string Currency { get; init; } = null!;
    public int NumberOfNights { get; init; }
    public decimal SubTotal { get; init; }
    public BookingItemStatus Status { get; init; }
}

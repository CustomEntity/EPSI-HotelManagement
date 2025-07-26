using System;

namespace HotelManagement.Application.DTOs.Booking;

public class BookingRoomSummaryDto
{
    public Guid RoomId { get; init; }
    public string RoomNumber { get; init; } = null!;
    public string RoomType { get; init; } = null!;
    public decimal PricePerNight { get; init; }
    public string Currency { get; init; } = null!;
} 
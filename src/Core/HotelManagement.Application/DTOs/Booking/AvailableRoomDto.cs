using System;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Application.DTOs.Booking;

public class AvailableRoomDto
{
    public Guid RoomId { get; init; }
    public string RoomNumber { get; init; } = null!;
    public string RoomType { get; init; } = null!;
    public string RoomStatus { get; init; } = null!;
    public string? RoomCondition { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public Money PricePerNight { get; set; }
    public int Capacity { get; set; }
}

using HotelManagement.Domain.Common;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;
using HotelManagement.Domain.Booking.ValueObjects;

namespace HotelManagement.Domain.Booking.Entities;

public class BookingItem : Entity
{
    public Guid Id { get; private set; }
    public RoomId RoomId { get; private set; }
    public Money PricePerNight { get; private set; }
    public GuestCount GuestCount { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private BookingItem() 
    {
        RoomId = null!;
        PricePerNight = null!;
        GuestCount = null!;
    }

    private BookingItem(RoomId roomId, Money pricePerNight, GuestCount guestCount)
    {
        Id = Guid.NewGuid();
        RoomId = roomId;
        PricePerNight = pricePerNight;
        GuestCount = guestCount;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<BookingItem> Create(RoomId roomId, Money pricePerNight, int adults, int children = 0)
    {
        var guestCountResult = GuestCount.Create(adults, children);
        if (guestCountResult.IsFailure)
            return Result<BookingItem>.Failure(guestCountResult.Error);

        if (pricePerNight.Amount <= 0)
            return Result<BookingItem>.Failure("Price per night must be greater than zero");

        return Result<BookingItem>.Success(new BookingItem(roomId, pricePerNight, guestCountResult.Value));
    }

    public Money CalculateTotalPrice(int nights)
    {
        return new Money(PricePerNight.Amount * nights, PricePerNight.Currency);
    }
}
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
    public BookingItemStatus Status { get; private set; }
    public DateTime? CheckInTime { get; private set; }
    public DateTime? CheckOutTime { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private BookingItem() 
    {
        RoomId = null!;
        PricePerNight = null!;
        GuestCount = null!;
        Status = null!;
    }

    private BookingItem(RoomId roomId, Money pricePerNight, GuestCount guestCount)
    {
        Id = Guid.NewGuid();
        RoomId = roomId;
        PricePerNight = pricePerNight;
        GuestCount = guestCount;
        Status = BookingItemStatus.Confirmed;
        CreatedAt = DateTime.UtcNow;
    }

    public Result CheckIn()
    {
        if (!Status.CanCheckIn())
            return Result.Failure($"Cannot check in item with status: {Status}");
            
        Status = BookingItemStatus.CheckedIn;
        CheckInTime = DateTime.UtcNow;
        return Result.Success();
    }

    public Result CheckOut()
    {
        if (!Status.CanCheckOut())
            return Result.Failure($"Cannot check out item with status: {Status}");
            
        Status = BookingItemStatus.CheckedOut;
        CheckOutTime = DateTime.UtcNow;
        return Result.Success();
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
}
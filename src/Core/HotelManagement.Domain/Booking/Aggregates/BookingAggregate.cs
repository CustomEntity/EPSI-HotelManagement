using HotelManagement.Domain.Common;
using HotelManagement.Domain.Booking.Entities;
using HotelManagement.Domain.Booking.Events;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Customer.ValueObjects;
using HotelManagement.Domain.Payment.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Booking.Aggregates;

public class Booking : AggregateRoot<BookingId>
{
    private readonly List<BookingItem> _items = new();
    
    public CustomerId CustomerId { get; private set; } = null!;
    public DateRange DateRange { get; private set; } = null!;
    public BookingStatus Status { get; private set; } = null!;
    public Money TotalAmount { get; private set; } = null!;
    public Money? DiscountAmount { get; private set; }
    public Money FinalAmount { get; private set; } = null!;
    public PaymentId? PaymentId { get; private set; }
    public CancellationPolicy CancellationPolicy { get; private set; } = null!;
    
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime? CheckInTime { get; private set; }
    public DateTime? CheckOutTime { get; private set; }
    
    public IReadOnlyCollection<BookingItem> Items => _items.AsReadOnly();

    private Booking() { }

    private Booking(
        CustomerId customerId,
        DateRange dateRange,
        CancellationPolicy cancellationPolicy)
    {
        Id = BookingId.New();
        CustomerId = customerId;
        DateRange = dateRange;
        Status = BookingStatus.Pending;
        CancellationPolicy = cancellationPolicy;
        CreatedAt = DateTime.UtcNow;
        TotalAmount = new Money(0, Currency.EUR);
        FinalAmount = new Money(0, Currency.EUR);
    }

    public static Result<Booking> Create(
        CustomerId customerId,
        DateRange dateRange,
        CancellationPolicy? cancellationPolicy = null)
    {
        var booking = new Booking(
            customerId,
            dateRange,
            cancellationPolicy ?? CancellationPolicy.Standard());

        booking.AddDomainEvent(new BookingCreatedEvent(
            booking.Id,
            customerId,
            dateRange,
            0)); // Sera mis à jour quand on ajoute des chambres

        return Result<Booking>.Success(booking);
    }

    public Result AddRoom(RoomId roomId, Money pricePerNight, int adults, int children = 0)
    {
        if (Status != BookingStatus.Pending)
            return Result.Failure("Cannot add rooms to a non-pending booking");

        // Vérifier si la chambre est déjà dans la réservation
        if (_items.Any(i => i.RoomId.Value == roomId.Value))
            return Result.Failure("Room is already in the booking");

        var itemResult = BookingItem.Create(roomId, pricePerNight, adults, children);
        if (itemResult.IsFailure)
            return Result.Failure(itemResult.Error);

        _items.Add(itemResult.Value);
        RecalculateTotals();
        ModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result RemoveRoom(RoomId roomId)
    {
        if (Status != BookingStatus.Pending)
            return Result.Failure("Cannot remove rooms from a non-pending booking");

        var item = _items.FirstOrDefault(i => i.RoomId.Value == roomId.Value);
        if (item == null)
            return Result.Failure("Room not found in booking");

        _items.Remove(item);
        RecalculateTotals();
        ModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result ApplyDiscount(decimal discountPercentage)
    {
        if (Status != BookingStatus.Pending)
            return Result.Failure("Cannot apply discount to a non-pending booking");

        if (discountPercentage < 0 || discountPercentage > 100)
            return Result.Failure("Discount percentage must be between 0 and 100");

        var discountAmount = TotalAmount.Amount * (discountPercentage / 100);
        DiscountAmount = new Money(discountAmount, TotalAmount.Currency);
        RecalculateTotals();
        ModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result ConfirmPayment(PaymentId paymentId)
    {
        if (Status != BookingStatus.Pending)
            return Result.Failure("Booking is not in pending status");

        if (!_items.Any())
            return Result.Failure("Cannot confirm booking without rooms");

        PaymentId = paymentId;
        Status = BookingStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new BookingConfirmedEvent(Id, paymentId));
        return Result.Success();
    }

    public Result Cancel(bool byReceptionist = false, string reason = "Customer request")
    {
        if (!Status.CanBeCancelled())
            return Result.Failure($"Booking cannot be cancelled in status: {Status}");

        var refundPercentage = CalculateRefundPercentage(byReceptionist);
        var isRefundable = refundPercentage > 0;

        Status = BookingStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        ModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new BookingCancelledEvent(Id, isRefundable, PaymentId, reason, refundPercentage));
        return Result.Success();
    }

    public Result CheckIn()
    {
        if (!Status.CanCheckIn())
            return Result.Failure($"Cannot check in with status: {Status}");

        if (DateTime.UtcNow.Date < DateRange.StartDate)
            return Result.Failure("Cannot check in before the booking start date");

        Status = BookingStatus.CheckedIn;
        CheckInTime = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;

        var roomIds = _items.Select(i => i.RoomId).ToList();
        AddDomainEvent(new GuestCheckedInEvent(Id, roomIds, CheckInTime.Value));

        return Result.Success();
    }

    public Result CheckOut()
    {
        if (!Status.CanCheckOut())
            return Result.Failure($"Cannot check out with status: {Status}");

        Status = BookingStatus.CheckedOut;
        CheckOutTime = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;

        var roomIds = _items.Select(i => i.RoomId).ToList();
        AddDomainEvent(new GuestCheckedOutEvent(Id, roomIds, CheckOutTime.Value));

        return Result.Success();
    }

    public Result MarkAsNoShow()
    {
        if (Status != BookingStatus.Confirmed)
            return Result.Failure("Can only mark confirmed bookings as no-show");

        if (DateTime.UtcNow.Date <= DateRange.StartDate)
            return Result.Failure("Cannot mark as no-show before or on the check-in date");

        Status = BookingStatus.NoShow;
        ModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private void RecalculateTotals()
    {
        var nights = DateRange.GetNights();
        var subtotal = _items.Sum(item => item.PricePerNight.Amount * nights);
        
        TotalAmount = new Money(subtotal, Currency.EUR);
        
        var discount = DiscountAmount?.Amount ?? 0;
        var finalAmount = subtotal - discount;
        
        FinalAmount = new Money(Math.Max(0, finalAmount), Currency.EUR);
    }

    private decimal CalculateRefundPercentage(bool byReceptionist)
    {
        if (Status == BookingStatus.Pending)
            return 100; // Réservation non confirmée = remboursement complet

        if (byReceptionist)
            return 100; // La réceptionniste peut forcer le remboursement

        return CancellationPolicy.CalculateRefundPercentage(DateRange.StartDate);
    }

    public bool IsUpcoming() => 
        Status == BookingStatus.Confirmed && DateRange.StartDate > DateTime.UtcNow.Date;

    public bool IsActive() => 
        Status == BookingStatus.CheckedIn;

    public bool IsPast() => 
        Status == BookingStatus.CheckedOut || DateRange.EndDate < DateTime.UtcNow.Date;

    public int GetTotalGuestCount() =>
        _items.Sum(i => i.GuestCount.Total);

    public List<RoomId> GetRoomIds() =>
        _items.Select(i => i.RoomId).ToList();
}

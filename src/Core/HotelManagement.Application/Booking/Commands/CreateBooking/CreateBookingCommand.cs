using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Booking.Commands.CreateBooking;

public sealed class CreateBookingCommand : IRequest<Result<Guid>>
{
    public Guid CustomerId { get; init; }
    
    public DateTime StartDate { get; init; }
    
    public DateTime EndDate { get; init; }
    
    public List<BookingRoomRequest> Rooms { get; init; } = new();
    
    public decimal? DiscountPercentage { get; init; }
    
    public string? SpecialRequests { get; init; }
    
    public bool RequirePayment { get; init; } = true;
    
    public CreditCardInfo? PaymentInfo { get; init; }
}

public sealed class BookingRoomRequest
{
    public Guid RoomId { get; init; }
    
    public int Adults { get; init; }
    
    public int Children { get; init; } = 0;
}

public sealed class CreditCardInfo
{
    public string CardNumber { get; init; } = string.Empty;
    
    public string CardholderName { get; init; } = string.Empty;
    
    public int ExpiryMonth { get; init; }
    
    public int ExpiryYear { get; init; }
    
    public string SecurityCode { get; init; } = string.Empty;
}
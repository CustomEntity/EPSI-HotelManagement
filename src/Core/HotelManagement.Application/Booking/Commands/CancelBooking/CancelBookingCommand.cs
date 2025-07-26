using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Booking.Commands.CancelBooking;

public sealed class CancelBookingCommand : IRequest<Result>
{
    public Guid BookingId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool ByReceptionist { get; set; } = false;

    public CancelBookingCommand(Guid bookingId, string reason = "Customer request", bool byReceptionist = false)
    {
        BookingId = bookingId;
        Reason = reason;
        ByReceptionist = byReceptionist;
    }

    public CancelBookingCommand() { }
}
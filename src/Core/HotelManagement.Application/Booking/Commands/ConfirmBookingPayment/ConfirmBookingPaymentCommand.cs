using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Booking.Commands.ConfirmBookingPayment;

public sealed class ConfirmBookingPaymentCommand : IRequest<Result>
{
    public Guid BookingId { get; init; }
    public Guid PaymentId { get; init; }
}
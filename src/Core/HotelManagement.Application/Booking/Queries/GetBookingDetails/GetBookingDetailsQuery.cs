using HotelManagement.Domain.Shared;
using MediatR;
using HotelManagement.Application.DTOs.Booking;

namespace HotelManagement.Application.Booking.Queries.GetBookingDetails;

public sealed class GetBookingDetailsQuery : IRequest<Result<BookingDetailsDto>>
{
    public Guid BookingId { get; init; }
}
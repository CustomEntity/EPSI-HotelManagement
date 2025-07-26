using HotelManagement.Domain.Shared;
using MediatR;
using HotelManagement.Application.DTOs.Booking;

namespace HotelManagement.Application.Booking.Queries.GetCustomerBookings;

public sealed class GetCustomerBookingsQuery : IRequest<Result<List<CustomerBookingDto>>>
{
    public Guid CustomerId { get; init; }
    public string? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
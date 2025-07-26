using HotelManagement.Domain.Booking;
using HotelManagement.Domain.Booking.Services;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Infrastructure.Features.Booking.Services;

public class BookingDomainService(IBookingRepository bookingRepository) : IBookingDomainService
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;

    public async Task<bool> IsRoomAvailableAsync(RoomId roomId, DateRange dateRange,
        CancellationToken cancellationToken = default)
    {
        return await _bookingRepository.IsRoomAvailableAsync(roomId, dateRange, cancellationToken: cancellationToken);
    }

    public Task<Result<Money>> CalculateBookingCostAsync(BookingRequest request,
        CancellationToken cancellationToken = default)
    {
        var totalCost = new Money(100 * request.Rooms.Count * request.DateRange.GetNights(), Currency.EUR);
        return Task.FromResult(Result<Money>.Success(totalCost));
    }

    public Task<Result<bool>> ValidateBookingRulesAsync(Domain.Booking.Aggregates.Booking booking,
        CancellationToken cancellationToken = default)
    {
        if (!booking.Items.Any())
            return Task.FromResult(Result<bool>.Failure("Booking must have at least one room"));

        if (booking.GetTotalGuestCount() == 0)
            return Task.FromResult(Result<bool>.Failure("Booking must have at least one guest"));

        return Task.FromResult(Result<bool>.Success(true));
    }
}
using HotelManagement.Domain.Booking;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Customer.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Infrastructure.Features.Booking.Repositories;

public class InMemoryBookingRepository : IBookingRepository
{
    private readonly List<Domain.Booking.Aggregates.Booking> _bookings = new();

    public Task<Domain.Booking.Aggregates.Booking?> GetByIdAsync(BookingId id, CancellationToken cancellationToken = default)
    {
        var booking = _bookings.FirstOrDefault(b => b.Id.Value == id.Value);
        return Task.FromResult(booking);
    }

    public Task<List<Domain.Booking.Aggregates.Booking>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_bookings.ToList());
    }

    public Task<Domain.Booking.Aggregates.Booking?> GetByIdWithItemsAsync(BookingId bookingId, CancellationToken cancellationToken = default)
    {
        return GetByIdAsync(bookingId, cancellationToken);
    }

    public Task<List<Domain.Booking.Aggregates.Booking>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default)
    {
        var bookings = _bookings
            .Where(b => b.CustomerId.Value == customerId.Value)
            .ToList();
        return Task.FromResult(bookings);
    }

    public Task<List<Domain.Booking.Aggregates.Booking>> GetByDateRangeAsync(DateRange dateRange, CancellationToken cancellationToken = default)
    {
        var bookings = _bookings
            .Where(b => b.DateRange.OverlapsWith(dateRange))
            .ToList();
        return Task.FromResult(bookings);
    }

    public Task<List<Domain.Booking.Aggregates.Booking>> GetActiveBookingsAsync(CancellationToken cancellationToken = default)
    {
        var activeBookings = _bookings
            .Where(b => b.IsActive())
            .ToList();
        return Task.FromResult(activeBookings);
    }

    public Task<List<Domain.Booking.Aggregates.Booking>> GetUpcomingBookingsAsync(CancellationToken cancellationToken = default)
    {
        var upcomingBookings = _bookings
            .Where(b => b.IsUpcoming())
            .ToList();
        return Task.FromResult(upcomingBookings);
    }

    public Task<bool> IsRoomAvailableAsync(RoomId roomId, DateRange dateRange, BookingId? excludeBookingId = null, CancellationToken cancellationToken = default)
    {
        var conflictingBookings = _bookings
            .Where(b => excludeBookingId == null || b.Id.Value != excludeBookingId.Value)
            .Where(b => b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.NoShow)
            .Where(b => b.GetRoomIds().Any(rid => rid.Value == roomId.Value))
            .Where(b => b.DateRange.OverlapsWith(dateRange))
            .ToList();

        return Task.FromResult(!conflictingBookings.Any());
    }

    public Task<List<RoomId>> GetBookedRoomIdsAsync(DateRange dateRange, CancellationToken cancellationToken = default)
    {
        var bookedRoomIds = _bookings
            .Where(b => b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.NoShow)
            .Where(b => b.DateRange.OverlapsWith(dateRange))
            .SelectMany(b => b.GetRoomIds())
            .Distinct()
            .ToList();

        return Task.FromResult(bookedRoomIds);
    }

    public Task<List<Domain.Booking.Aggregates.Booking>> GetCheckInsForTodayAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var checkIns = _bookings
            .Where(b => b.Status == BookingStatus.Confirmed)
            .Where(b => b.DateRange.StartDate == today)
            .ToList();
        return Task.FromResult(checkIns);
    }

    public Task<List<Domain.Booking.Aggregates.Booking>> GetCheckOutsForTodayAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var checkOuts = _bookings
            .Where(b => b.Status == BookingStatus.CheckedIn)
            .Where(b => b.DateRange.EndDate == today)
            .ToList();
        return Task.FromResult(checkOuts);
    }

    public Task<List<Domain.Booking.Aggregates.Booking>> GetBookingsRequiringPreStayNotificationAsync(CancellationToken cancellationToken = default)
    {
        var bookings = _bookings
            .Where(b => b.Status == BookingStatus.Confirmed)
            .Where(b => b.DateRange.IsWithinNext48Hours())
            .ToList();
        return Task.FromResult(bookings);
    }

    public Task AddAsync(Domain.Booking.Aggregates.Booking entity, CancellationToken cancellationToken = default)
    {
        _bookings.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Domain.Booking.Aggregates.Booking entity, CancellationToken cancellationToken = default)
    {
        var existingBooking = _bookings.FirstOrDefault(b => b.Id.Value == entity.Id.Value);
        if (existingBooking != null)
        {
            _bookings.Remove(existingBooking);
            _bookings.Add(entity);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Domain.Booking.Aggregates.Booking entity, CancellationToken cancellationToken = default)
    {
        var booking = _bookings.FirstOrDefault(b => b.Id.Value == entity.Id.Value);
        if (booking != null)
        {
            _bookings.Remove(booking);
        }
        return Task.CompletedTask;
    }
}
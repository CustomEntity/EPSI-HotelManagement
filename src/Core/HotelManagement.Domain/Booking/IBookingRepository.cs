using HotelManagement.Domain.Common;
using HotelManagement.Domain.Booking.Aggregates;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Customer.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Domain.Booking;

public interface IBookingRepository : IRepository<Aggregates.Booking, BookingId>
{
    Task<Aggregates.Booking?> GetByIdWithItemsAsync(BookingId bookingId, CancellationToken cancellationToken = default);
    Task<List<Aggregates.Booking>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default);
    Task<List<Aggregates.Booking>> GetByDateRangeAsync(DateRange dateRange, CancellationToken cancellationToken = default);
    Task<List<Aggregates.Booking>> GetActiveBookingsAsync(CancellationToken cancellationToken = default);
    Task<List<Aggregates.Booking>> GetUpcomingBookingsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsRoomAvailableAsync(RoomId roomId, DateRange dateRange, BookingId? excludeBookingId = null, CancellationToken cancellationToken = default);
    Task<List<RoomId>> GetBookedRoomIdsAsync(DateRange dateRange, CancellationToken cancellationToken = default);
    Task<List<Aggregates.Booking>> GetCheckInsForTodayAsync(CancellationToken cancellationToken = default);
    Task<List<Aggregates.Booking>> GetCheckOutsForTodayAsync(CancellationToken cancellationToken = default);
    Task<List<Aggregates.Booking>> GetBookingsRequiringPreStayNotificationAsync(CancellationToken cancellationToken = default);
}
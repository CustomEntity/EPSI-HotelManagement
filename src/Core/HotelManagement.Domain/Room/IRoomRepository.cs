using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Room.Aggregates;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Domain.Room;

public interface IRoomRepository : IRepository<Aggregates.Room, RoomId>
{
    Task<Aggregates.Room?> GetByNumberAsync(RoomNumber number, CancellationToken cancellationToken = default);
    Task<List<Aggregates.Room>> GetAvailableRoomsAsync(CancellationToken cancellationToken = default);

    Task<List<Aggregates.Room>> GetAvailableRoomsInDateRangeAsync(
        DateRange dateRange,
        CancellationToken cancellationToken = default);
}
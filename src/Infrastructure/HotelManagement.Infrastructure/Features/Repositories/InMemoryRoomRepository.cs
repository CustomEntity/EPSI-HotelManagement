using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Room;
using HotelManagement.Domain.Room.Aggregates;
using HotelManagement.Domain.Room.Entities;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Infrastructure.Features.Repositories;

public class InMemoryRoomRepository : IRoomRepository
{
    private readonly List<Room> _rooms = new();

    public InMemoryRoomRepository()
    {
        SeedTestData();
    }

    public Task<Room?> GetByIdAsync(RoomId id, CancellationToken cancellationToken = default)
    {
        var room = _rooms.FirstOrDefault(r => r.Id.Value == id.Value);
        return Task.FromResult(room);
    }

    public Task<List<Room>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_rooms.ToList());
    }

    public Task<Room?> GetByNumberAsync(RoomNumber number,
        CancellationToken cancellationToken = default)
    {
        var room = _rooms.FirstOrDefault(r => r.Number.Value == number.Value);
        return Task.FromResult(room);
    }

    public Task<List<Room>> GetAvailableRoomsAsync(CancellationToken cancellationToken = default)
    {
        var availableRooms = _rooms
            .Where(r => r.Status == RoomStatus.Available)
            .ToList();

        return Task.FromResult(availableRooms);
    }

    public Task<List<Room>> GetAvailableRoomsInDateRangeAsync(
        DateRange dateRange,
        CancellationToken cancellationToken = default)
    {
        // Pour le moment, retournons toutes les chambres disponibles
        // Plus tard, vous vérifierez les réservations avec IBookingRepository
        var availableRooms = _rooms
            .Where(r => r.Status == RoomStatus.Available)
            .ToList();

        return Task.FromResult(availableRooms);
    }

    public Task AddAsync(Room entity, CancellationToken cancellationToken = default)
    {
        _rooms.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Room entity, CancellationToken cancellationToken = default)
    {
        var existingRoom = _rooms.FirstOrDefault(r => r.Id.Value == entity.Id.Value);
        if (existingRoom != null)
        {
            _rooms.Remove(existingRoom);
            _rooms.Add(entity);
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Room entity, CancellationToken cancellationToken = default)
    {
        var room = _rooms.FirstOrDefault(r => r.Id.Value == entity.Id.Value);
        if (room != null)
        {
            _rooms.Remove(room);
        }

        return Task.CompletedTask;
    }

    private void SeedTestData()
    {
        try
        {
            // Créez quelques chambres de test
            var roomType1 = CreateTestRoomType("Standard", new Money(100, Currency.EUR));
            var roomType2 = CreateTestRoomType("Deluxe", new Money(150, Currency.EUR));

            var room1 = CreateTestRoom("101", roomType1);
            var room2 = CreateTestRoom("102", roomType1);
            var room3 = CreateTestRoom("201", roomType2);

            if (room1 != null) _rooms.Add(room1);
            if (room2 != null) _rooms.Add(room2);
            if (room3 != null) _rooms.Add(room3);
        }
        catch
        {
        }
    }

    private RoomType CreateTestRoomType(string name, Money basePrice)
    {
        return new RoomType(name, basePrice, maxOccupancy: 2);
    }

    private Room? CreateTestRoom(string number, RoomType type)
    {
        try
        {
            var roomNumber = RoomNumber.Create(number);
            if (roomNumber.IsFailure) return null;

            return new Room(
                roomNumber.Value,
                type,
                RoomCondition.Good); // ou la valeur par défaut appropriée
        }
        catch
        {
            return null;
        }
    }
}
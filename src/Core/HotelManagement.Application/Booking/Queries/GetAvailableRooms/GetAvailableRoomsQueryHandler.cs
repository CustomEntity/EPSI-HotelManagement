using HotelManagement.Domain.Booking;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Room;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Booking.Queries.GetAvailableRooms;

public sealed class GetAvailableRoomsQueryHandler : IRequestHandler<GetAvailableRoomsQuery, Result<List<AvailableRoomDto>>>
{
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;

    public GetAvailableRoomsQueryHandler(
        IRoomRepository roomRepository,
        IBookingRepository bookingRepository)
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<List<AvailableRoomDto>>> Handle(
        GetAvailableRoomsQuery request, 
        CancellationToken cancellationToken)
    {
        // Valider la période de dates
        var dateRangeResult = DateRange.Create(request.StartDate, request.EndDate);
        if (dateRangeResult.IsFailure)
            return Result<List<AvailableRoomDto>>.Failure(dateRangeResult.Error);

        var dateRange = dateRangeResult.Value;

        // Récupérer toutes les chambres disponibles (statut Available)
        var availableRooms = await _roomRepository.GetRoomsByStatusAsync(
            RoomStatus.Available, 
            cancellationToken);

        // Filtrer par capacité si spécifiée
        if (request.MinOccupancy.HasValue)
        {
            availableRooms = availableRooms
                .Where(r => r.Type.MaxOccupancy >= request.MinOccupancy.Value)
                .ToList();
        }

        if (request.MaxOccupancy.HasValue)
        {
            availableRooms = availableRooms
                .Where(r => r.Type.MaxOccupancy <= request.MaxOccupancy.Value)
                .ToList();
        }

        // Vérifier la disponibilité pour chaque chambre (pas de réservation conflictuelle)
        var result = new List<AvailableRoomDto>();

        foreach (var room in availableRooms)
        {
            var isAvailable = await _bookingRepository.IsRoomAvailableAsync(
                room.Id, 
                dateRange, 
                cancellationToken: cancellationToken);

            if (isAvailable)
            {
                result.Add(new AvailableRoomDto
                {
                    RoomId = room.Id.Value,
                    RoomNumber = room.Number.ToString(),
                    RoomTypeName = room.Type.Name,
                    PricePerNight = room.Type.BasePrice.Amount,
                    Currency = room.Type.BasePrice.Currency.Code,
                    MaxOccupancy = room.Type.MaxOccupancy,
                    Status = room.Status.Value,
                    Condition = room.Condition.Value
                });
            }
        }

        return Result<List<AvailableRoomDto>>.Success(result);
    }
}
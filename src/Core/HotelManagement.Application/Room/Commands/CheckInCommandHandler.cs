using HotelManagement.Domain.Booking;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Room;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Room.Commands.CheckIn;

public sealed class CheckInCommandHandler : IRequestHandler<CheckInCommand, Result>
{
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CheckInCommandHandler(
        IRoomRepository roomRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork)
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        var roomId = new RoomId(request.RoomId);
        var bookingId = BookingId.Create(request.BookingId);

        // Récupérer la chambre
        var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken);
        if (room == null)
            return Result.Failure("Room not found");

        // Récupérer la réservation
        var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
        if (booking == null)
            return Result.Failure("Booking not found");

        // Vérifier que la réservation contient cette chambre
        if (!booking.GetRoomIds().Contains(roomId))
            return Result.Failure("This room is not part of the specified booking");

        // Vérifier que la réservation est confirmée
        if (booking.Status != BookingStatus.Confirmed)
            return Result.Failure($"Booking must be confirmed before check-in. Current status: {booking.Status}");

        // Vérifier que c'est le bon jour pour le check-in
        var checkInDate = request.CheckInTime.Date;
        if (checkInDate < booking.DateRange.StartDate.Date)
            return Result.Failure("Check-in date is before the booking start date");

        if (checkInDate > booking.DateRange.StartDate.Date.AddDays(1))
            return Result.Failure("Check-in date is too late. Contact front desk for late check-in");

        // Vérifier que c'est la seule chambre de la réservation
        var roomIds = booking.GetRoomIds();
        if (roomIds.Count > 1)
            return Result.Failure("This booking has multiple rooms. Please use the multi-room check-in process.");

        // Effectuer le check-in de la chambre
        var checkInResult = room.CheckIn();
        if (checkInResult.IsFailure)
            return checkInResult;

        // Mettre à jour la réservation avec l'heure de check-in
        var updateBookingResult = booking.CheckIn();
        if (updateBookingResult.IsFailure)
            return updateBookingResult;

        // Sauvegarder les changements
        await _roomRepository.UpdateAsync(room, cancellationToken);
        await _bookingRepository.UpdateAsync(booking, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
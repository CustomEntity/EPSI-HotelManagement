using HotelManagement.Domain.Booking;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Room;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Room.Commands.CheckOut;

public sealed class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, Result>
{
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CheckOutCommandHandler(
        IRoomRepository roomRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork)
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CheckOutCommand request, CancellationToken cancellationToken)
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

        // Vérifier que la réservation peut faire des check-outs
        if (!booking.Status.CanCheckOut())
            return Result.Failure($"Booking cannot check out with status: {booking.Status}");

        // Vérifier que c'est le bon moment pour le check-out
        var checkOutDate = request.CheckOutTime.Date;
        if (checkOutDate < booking.DateRange.StartDate.Date)
            return Result.Failure("Check-out date cannot be before the booking start date");

        if (checkOutDate > booking.DateRange.EndDate.Date.AddDays(1))
            return Result.Failure("Check-out date is too late. Contact front desk for late check-out");

        // Vérifier que la chambre est actuellement occupée
        if (room.Status != RoomStatus.Occupied)
            return Result.Failure($"Room cannot be checked out. Current status: {room.Status}");

        // Effectuer le check-out de la chambre
        var checkOutResult = room.CheckOut();
        if (checkOutResult.IsFailure)
            return checkOutResult;

        // Check-out granulaire au niveau réservation
        var updateBookingResult = booking.CheckOutRoom(roomId);
        if (updateBookingResult.IsFailure)
            return updateBookingResult;

        // Sauvegarder les changements
        await _roomRepository.UpdateAsync(room, cancellationToken);
        await _bookingRepository.UpdateAsync(booking, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
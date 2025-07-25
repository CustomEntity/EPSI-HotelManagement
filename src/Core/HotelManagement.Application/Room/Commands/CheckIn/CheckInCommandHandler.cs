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

        var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken);
        if (room == null)
            return Result.Failure("Room not found");

        var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
        if (booking == null)
            return Result.Failure("Booking not found");

        if (!booking.GetRoomIds().Contains(roomId))
            return Result.Failure("This room is not part of the specified booking");

        if (!booking.Status.CanCheckIn())
            return Result.Failure($"Booking cannot check in with status: {booking.Status}");

        var checkInDate = request.CheckInTime.Date;
        if (checkInDate < booking.DateRange.StartDate.Date)
            return Result.Failure("Check-in date is before the booking start date");

        if (checkInDate > booking.DateRange.StartDate.Date.AddDays(1))
            return Result.Failure("Check-in date is too late. Contact front desk for late check-in");

        var checkInResult = room.CheckIn();
        if (checkInResult.IsFailure)
            return checkInResult;

        var updateBookingResult = booking.CheckInRoom(roomId);
        if (updateBookingResult.IsFailure)
            return updateBookingResult;

        await _roomRepository.UpdateAsync(room, cancellationToken);
        await _bookingRepository.UpdateAsync(booking, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
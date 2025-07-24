using HotelManagement.Domain.Booking;
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Booking.Services;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Customer.ValueObjects;
using HotelManagement.Domain.Room;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Booking.Commands.BookRoom;

public sealed class BookRoomCommandHandler : IRequestHandler<BookRoomCommand, Result<Guid>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingDomainService _bookingDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public BookRoomCommandHandler(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository,
        IBookingDomainService bookingDomainService,
        IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _bookingDomainService = bookingDomainService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(BookRoomCommand request, CancellationToken cancellationToken)
    {
        var customerId = CustomerId.Create(request.CustomerId);
        var roomId = new RoomId(request.RoomId);
        
        var dateRangeResult = DateRange.Create(request.StartDate, request.EndDate);
        if (dateRangeResult.IsFailure)
            return Result<Guid>.Failure(dateRangeResult.Error);

        var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken);
        if (room == null)
            return Result<Guid>.Failure("Room not found");

        var isAvailable = await _bookingRepository.IsRoomAvailableAsync(
            roomId, 
            dateRangeResult.Value, 
            cancellationToken: cancellationToken);
        
        if (!isAvailable)
            return Result<Guid>.Failure("Room is not available for the selected dates");

        var bookingResult = Domain.Booking.Aggregates.Booking.Create(
            customerId,
            dateRangeResult.Value);
        
        if (bookingResult.IsFailure)
            return Result<Guid>.Failure(bookingResult.Error);

        var booking = bookingResult.Value;

        var roomPrice = room.Type.BasePrice;
        var pricePerNight = new Money(roomPrice.Amount, roomPrice.Currency);

        var addRoomResult = booking.AddRoom(roomId, pricePerNight, request.Adults, request.Children);
        if (addRoomResult.IsFailure)
            return Result<Guid>.Failure(addRoomResult.Error);

        if (request.DiscountPercentage.HasValue && request.DiscountPercentage.Value > 0)
        {
            var discountResult = booking.ApplyDiscount(request.DiscountPercentage.Value);
            if (discountResult.IsFailure)
                return Result<Guid>.Failure(discountResult.Error);
        }

        var validationResult = await _bookingDomainService.ValidateBookingRulesAsync(
            booking, 
            cancellationToken);
        
        if (validationResult.IsFailure)
            return Result<Guid>.Failure(validationResult.Error);

        await _bookingRepository.AddAsync(booking, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(booking.Id.Value);
    }
}
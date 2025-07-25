using HotelManagement.Application.DTOs.Booking;
using HotelManagement.Domain.Booking;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Room;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Booking.Queries.GetBookingDetails;

public sealed class GetBookingDetailsQueryHandler : IRequestHandler<GetBookingDetailsQuery, Result<BookingDetailsDto>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;

    public GetBookingDetailsQueryHandler(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
    }

    public async Task<Result<BookingDetailsDto>> Handle(
        GetBookingDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var bookingId = BookingId.Create(request.BookingId);
        
        var booking = await _bookingRepository.GetByIdWithItemsAsync(bookingId, cancellationToken);
        if (booking == null)
            return Result<BookingDetailsDto>.Failure("Booking not found");

        var bookingItems = new List<BookingItemDto>();
        
        foreach (var item in booking.Items)
        {
            var room = await _roomRepository.GetByIdAsync(item.RoomId, cancellationToken);
            if (room != null)
            {
                bookingItems.Add(new BookingItemDto
                {
                    BookingItemId = item.Id,
                    RoomId = item.RoomId.Value,
                    RoomNumber = room.Number.Value,
                    RoomType = room.Type.Name,
                    PricePerNight = item.PricePerNight.Amount,
                    Currency = item.PricePerNight.Currency.Code,
                    NumberOfNights = booking.DateRange.GetNights(),
                    SubTotal = item.PricePerNight.Amount * booking.DateRange.GetNights()
                });
            }
        }

        var bookingDetailsDto = new BookingDetailsDto
        {
            BookingId = booking.Id.Value,
            CustomerId = booking.CustomerId.Value,
            StartDate = booking.DateRange.StartDate,
            EndDate = booking.DateRange.EndDate,
            Status = booking.Status.ToString(),
            TotalAmount = booking.TotalAmount.Amount,
            Currency = booking.TotalAmount.Currency.Code,
            DiscountAmount = booking.DiscountAmount?.Amount,
            FinalAmount = booking.FinalAmount.Amount,
            PaymentId = booking.PaymentId?.Value,
            CancellationPolicy = booking.CancellationPolicy.ToString(),
            ConfirmedAt = booking.ConfirmedAt,
            CancelledAt = booking.CancelledAt,
            CancellationReason = booking.CancellationReason,
            CheckInTime = booking.CheckInTime,
            CheckOutTime = booking.CheckOutTime,
            CreatedAt = booking.CreatedAt,
            ModifiedAt = booking.ModifiedAt,
            Items = bookingItems
        };

        return Result<BookingDetailsDto>.Success(bookingDetailsDto);
    }
}
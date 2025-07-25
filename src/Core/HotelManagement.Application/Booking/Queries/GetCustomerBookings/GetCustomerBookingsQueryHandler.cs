using HotelManagement.Domain.Booking;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Customer.ValueObjects;
using HotelManagement.Domain.Room;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Booking.Queries.GetCustomerBookings;

public sealed class GetCustomerBookingsQueryHandler : IRequestHandler<GetCustomerBookingsQuery, Result<List<CustomerBookingDto>>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;

    public GetCustomerBookingsQueryHandler(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
    }

    public async Task<Result<List<CustomerBookingDto>>> Handle(
        GetCustomerBookingsQuery request,
        CancellationToken cancellationToken)
    {
        var customerId = CustomerId.Create(request.CustomerId);
        
        var bookings = await _bookingRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        
        // Filtrer par statut si spécifié
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<BookingStatus>(request.Status, true, out var status))
            {
                bookings = bookings.Where(b => b.Status == status).ToList();
            }
        }

        // Filtrer par dates si spécifiées
        if (request.FromDate.HasValue)
        {
            bookings = bookings.Where(b => b.DateRange.StartDate >= request.FromDate.Value).ToList();
        }

        if (request.ToDate.HasValue)
        {
            bookings = bookings.Where(b => b.DateRange.EndDate <= request.ToDate.Value).ToList();
        }

        // Trier par date de création (plus récent en premier)
        bookings = bookings.OrderByDescending(b => b.CreatedAt).ToList();

        // Pagination
        var totalCount = bookings.Count;
        var pagedBookings = bookings
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var customerBookingDtos = new List<CustomerBookingDto>();

        foreach (var booking in pagedBookings)
        {
            var roomSummaries = new List<BookingRoomSummaryDto>();

            // Récupérer les détails des chambres pour chaque item
            foreach (var item in booking.Items)
            {
                var room = await _roomRepository.GetByIdAsync(item.RoomId, cancellationToken);
                if (room != null)
                {
                    roomSummaries.Add(new BookingRoomSummaryDto
                    {
                        RoomId = item.RoomId.Value,
                        RoomNumber = room.Number.Value,
                        RoomType = room.Type.Name,
                        PricePerNight = item.PricePerNight.Amount,
                        Currency = item.PricePerNight.Currency.Code
                    });
                }
            }

            var customerBookingDto = new CustomerBookingDto
            {
                BookingId = booking.Id.Value,
                StartDate = booking.DateRange.StartDate,
                EndDate = booking.DateRange.EndDate,
                Status = booking.Status.ToString(),
                TotalAmount = booking.TotalAmount.Amount,
                Currency = booking.TotalAmount.Currency.Code,
                FinalAmount = booking.FinalAmount.Amount,
                CancellationPolicy = booking.CancellationPolicy.ToString(),
                ConfirmedAt = booking.ConfirmedAt,
                CancelledAt = booking.CancelledAt,
                CancellationReason = booking.CancellationReason,
                CheckInTime = booking.CheckInTime,
                CheckOutTime = booking.CheckOutTime,
                CreatedAt = booking.CreatedAt,
                NumberOfRooms = booking.Items.Count,
                NumberOfNights = booking.DateRange.GetNumberOfNights(),
                Rooms = roomSummaries
            };

            customerBookingDtos.Add(customerBookingDto);
        }

        return Result<List<CustomerBookingDto>>.Success(customerBookingDtos);
    }
}
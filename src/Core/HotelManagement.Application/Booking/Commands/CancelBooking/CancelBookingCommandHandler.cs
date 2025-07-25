using HotelManagement.Domain.Booking;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Application.Booking.Commands.CancelBooking;

public sealed class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, Result>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelBookingCommandHandler> _logger;

    public CancelBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        ILogger<CancelBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting booking cancellation for BookingId: {BookingId}", request.BookingId);

            var bookingId = new BookingId(request.BookingId);
            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning("Booking not found with ID: {BookingId}", request.BookingId);
                return Result.Failure("Booking not found");
            }

            var cancelResult = booking.Cancel(request.ByReceptionist, request.Reason);
            if (cancelResult.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to cancel booking {BookingId}: {Error}", 
                    request.BookingId, 
                    cancelResult.Error);
                return cancelResult;
            }

            await _bookingRepository.UpdateAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully cancelled booking {BookingId} by {CancelledBy} with reason: {Reason}",
                request.BookingId,
                request.ByReceptionist ? "Receptionist" : "Customer",
                request.Reason);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling booking {BookingId}", request.BookingId);
            return Result.Failure("An error occurred while cancelling the booking");
        }
    }
}
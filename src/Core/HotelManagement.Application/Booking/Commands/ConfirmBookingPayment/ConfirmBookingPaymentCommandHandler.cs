using HotelManagement.Domain.Booking;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Payment;
using HotelManagement.Domain.Payment.ValueObjects;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Booking.Commands.ConfirmBookingPayment;

public sealed class ConfirmBookingPaymentCommandHandler : IRequestHandler<ConfirmBookingPaymentCommand, Result>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmBookingPaymentCommandHandler(
        IBookingRepository bookingRepository,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ConfirmBookingPaymentCommand request, CancellationToken cancellationToken)
    {
        var bookingId = BookingId.Create(request.BookingId);
        var paymentId = PaymentId.Create(request.PaymentId);

        var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
        if (booking == null)
            return Result.Failure("Booking not found");

        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
            return Result.Failure("Payment not found");

        if (payment.BookingId.Value != booking.Id.Value)
            return Result.Failure("Payment does not belong to this booking");

        if (payment.Status != PaymentStatus.Completed)
            return Result.Failure("Payment is not in completed status");

        var confirmResult = booking.ConfirmPayment(paymentId);
        if (confirmResult.IsFailure)
            return Result.Failure(confirmResult.Error);

        await _bookingRepository.UpdateAsync(booking, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
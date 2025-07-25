using FluentValidation;

namespace HotelManagement.Application.Booking.Commands.ConfirmBookingPayment;

public sealed class ConfirmBookingPaymentCommandValidator : AbstractValidator<ConfirmBookingPaymentCommand>
{
    public ConfirmBookingPaymentCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty()
            .WithMessage("Booking ID is required");

        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required");
    }
}
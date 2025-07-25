using FluentValidation;

namespace HotelManagement.Application.Room.Commands.CheckOut;

public sealed class CheckOutCommandValidator : AbstractValidator<CheckOutCommand>
{
    public CheckOutCommandValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty()
            .WithMessage("Room ID is required");

        RuleFor(x => x.BookingId)
            .NotEmpty()
            .WithMessage("Booking ID is required");

        RuleFor(x => x.CheckOutTime)
            .NotEmpty()
            .WithMessage("Check-out time is required")
            .Must(BeWithinReasonableTimeRange)
            .WithMessage("Check-out time must be within a reasonable range (not more than 7 days in the future or past)")
            .Must(NotBeInFuture)
            .WithMessage("Check-out time cannot be in the future");
    }

    private bool BeWithinReasonableTimeRange(DateTime checkOutTime)
    {
        var now = DateTime.UtcNow;
        var sevenDaysInPast = now.AddDays(-7);
        var oneDayInFuture = now.AddDays(1);
        
        return checkOutTime >= sevenDaysInPast && checkOutTime <= oneDayInFuture;
    }

    private bool NotBeInFuture(DateTime checkOutTime)
    {
        return checkOutTime <= DateTime.UtcNow.AddHours(1);
    }
}
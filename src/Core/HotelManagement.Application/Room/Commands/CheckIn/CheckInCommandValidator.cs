using FluentValidation;

namespace HotelManagement.Application.Room.Commands.CheckIn;

public sealed class CheckInCommandValidator : AbstractValidator<CheckInCommand>
{
    public CheckInCommandValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty()
            .WithMessage("Room ID is required");

        RuleFor(x => x.BookingId)
            .NotEmpty()
            .WithMessage("Booking ID is required");

        RuleFor(x => x.CheckInTime)
            .NotEmpty()
            .WithMessage("Check-in time is required")
            .Must(BeWithinReasonableTimeRange)
            .WithMessage("Check-in time must be within a reasonable range (not more than 1 day in the future or past)");
    }

    private bool BeWithinReasonableTimeRange(DateTime checkInTime)
    {
        var now = DateTime.UtcNow;
        var oneDayInFuture = now.AddDays(1);
        var oneDayInPast = now.AddDays(-1);
        
        return checkInTime >= oneDayInPast && checkInTime <= oneDayInFuture;
    }
}
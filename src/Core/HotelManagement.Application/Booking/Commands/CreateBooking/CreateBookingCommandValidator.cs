using FluentValidation;

namespace HotelManagement.Application.Booking.Commands.BookRoom;

public sealed class BookRoomCommandValidator : AbstractValidator<BookRoomCommand>
{
    public BookRoomCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.RoomId)
            .NotEmpty()
            .WithMessage("Room ID is required");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Start date cannot be in the past");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x)
            .Must(x => (x.EndDate - x.StartDate).TotalDays <= 365)
            .WithMessage("Booking duration cannot exceed 365 days")
            .When(x => x.StartDate != default && x.EndDate != default);

        RuleFor(x => x.Adults)
            .GreaterThan(0)
            .WithMessage("At least one adult is required")
            .LessThanOrEqualTo(4)
            .WithMessage("Maximum 4 adults per room");

        RuleFor(x => x.Children)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Children count cannot be negative")
            .LessThanOrEqualTo(3)
            .WithMessage("Maximum 3 children per room");

        RuleFor(x => x)
            .Must(x => x.Adults + x.Children <= 6)
            .WithMessage("Total guests cannot exceed 6 per room");

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100)
            .WithMessage("Discount percentage must be between 0 and 100")
            .When(x => x.DiscountPercentage.HasValue);

        RuleFor(x => x.SpecialRequests)
            .MaximumLength(500)
            .WithMessage("Special requests cannot exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.SpecialRequests));

        When(x => x.RequirePayment && x.PaymentInfo != null, () =>
        {
            RuleFor(x => x.PaymentInfo!.CardNumber)
                .NotEmpty()
                .WithMessage("Card number is required")
                .CreditCard()
                .WithMessage("Invalid credit card number");

            RuleFor(x => x.PaymentInfo!.CardholderName)
                .NotEmpty()
                .WithMessage("Cardholder name is required")
                .MaximumLength(100)
                .WithMessage("Cardholder name cannot exceed 100 characters");

            RuleFor(x => x.PaymentInfo!.ExpiryMonth)
                .InclusiveBetween(1, 12)
                .WithMessage("Expiry month must be between 1 and 12");

            RuleFor(x => x.PaymentInfo!.ExpiryYear)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Year)
                .WithMessage("Card has expired");

            RuleFor(x => x.PaymentInfo!)
                .Must(x => IsCardNotExpired(x.ExpiryMonth, x.ExpiryYear))
                .WithMessage("Card has expired")
                .WithName("Card expiry");

            RuleFor(x => x.PaymentInfo!.SecurityCode)
                .NotEmpty()
                .WithMessage("Security code is required")
                .Matches(@"^\d{3,4}$")
                .WithMessage("Security code must be 3 or 4 digits");
        });

        When(x => x.RequirePayment && x.PaymentInfo == null, () =>
        {
            RuleFor(x => x.PaymentInfo)
                .NotNull()
                .WithMessage("Payment information is required when payment is required");
        });
    }

    private bool IsCardNotExpired(int month, int year)
    {
        var now = DateTime.UtcNow;
        var currentYear = now.Year;
        var currentMonth = now.Month;

        if (year > currentYear)
            return true;

        if (year == currentYear && month >= currentMonth)
            return true;

        return false;
    }
}
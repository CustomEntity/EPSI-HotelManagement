using FluentValidation;

namespace HotelManagement.Application.Payment.Commands.ProcessRefund;

public sealed class ProcessRefundCommandValidator : AbstractValidator<ProcessRefundCommand>
{
    public ProcessRefundCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Refund amount must be greater than zero")
            .LessThanOrEqualTo(999999.99m)
            .WithMessage("Refund amount cannot exceed 999,999.99");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a 3-letter code (e.g., EUR, USD)")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be in uppercase format");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Refund reason is required")
            .MaximumLength(500)
            .WithMessage("Refund reason cannot exceed 500 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
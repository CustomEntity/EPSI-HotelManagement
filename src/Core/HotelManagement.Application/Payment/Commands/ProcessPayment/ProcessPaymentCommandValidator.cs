using FluentValidation;

namespace HotelManagement.Application.Payment.Commands.ProcessPayment;

public sealed class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty()
            .WithMessage("BookingId is required");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("CustomerId is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency must be a valid 3-letter code");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .Must(BeValidPaymentMethod)
            .WithMessage("PaymentMethod must be one of: CreditCard, DebitCard, Cash, BankTransfer, PayPal");

        When(x => RequiresOnlineProcessing(x.PaymentMethod), () =>
        {
            RuleFor(x => x.CardNumber)
                .NotEmpty()
                .WithMessage("Card number is required for online payments")
                .Must(BeValidCardNumber)
                .WithMessage("Invalid card number format");

            RuleFor(x => x.CardHolderName)
                .NotEmpty()
                .WithMessage("Card holder name is required for online payments")
                .MaximumLength(100)
                .WithMessage("Card holder name cannot exceed 100 characters");

            RuleFor(x => x.ExpiryMonth)
                .NotNull()
                .InclusiveBetween(1, 12)
                .WithMessage("Expiry month must be between 1 and 12");

            RuleFor(x => x.ExpiryYear)
                .NotNull()
                .GreaterThanOrEqualTo(DateTime.Now.Year)
                .WithMessage("Expiry year must be current year or later");

            RuleFor(x => x.CVV)
                .NotEmpty()
                .WithMessage("CVV is required for online payments")
                .Must(BeValidCVV)
                .WithMessage("CVV must be 3 or 4 digits");
        });

        When(x => x.ExpiryMonth.HasValue && x.ExpiryYear.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(x => !IsCardExpired(x.ExpiryMonth!.Value, x.ExpiryYear!.Value))
                .WithMessage("Card has expired");
        });
    }

    private static bool BeValidPaymentMethod(string paymentMethod)
    {
        var validMethods = new[] { "CreditCard", "DebitCard", "Cash", "BankTransfer", "PayPal" };
        return validMethods.Contains(paymentMethod);
    }

    private static bool RequiresOnlineProcessing(string paymentMethod)
    {
        var onlineMethods = new[] { "CreditCard", "DebitCard", "PayPal" };
        return onlineMethods.Contains(paymentMethod);
    }

    private static bool BeValidCardNumber(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            return false;

        var cleanNumber = cardNumber.Replace(" ", "").Replace("-", "");
        
        if (!cleanNumber.All(char.IsDigit))
            return false;

        if (cleanNumber.Length < 13 || cleanNumber.Length > 19)
            return false;

        return IsValidLuhn(cleanNumber);
    }

    private static bool BeValidCVV(string? cvv)
    {
        if (string.IsNullOrWhiteSpace(cvv))
            return false;

        return cvv.All(char.IsDigit) && (cvv.Length == 3 || cvv.Length == 4);
    }

    private static bool IsCardExpired(int month, int year)
    {
        var now = DateTime.Now;
        var cardExpiry = new DateTime(year, month, DateTime.DaysInMonth(year, month));
        return cardExpiry < now.Date;
    }

    private static bool IsValidLuhn(string cardNumber)
    {
        int sum = 0;
        bool alternate = false;

        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            int digit = int.Parse(cardNumber[i].ToString());

            if (alternate)
            {
                digit *= 2;
                if (digit > 9)
                    digit = (digit % 10) + 1;
            }

            sum += digit;
            alternate = !alternate;
        }

        return (sum % 10) == 0;
    }
}
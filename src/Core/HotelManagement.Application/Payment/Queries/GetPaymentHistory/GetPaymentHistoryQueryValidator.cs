using FluentValidation;

namespace HotelManagement.Application.Payment.Queries.GetPaymentHistory;

public sealed class GetPaymentHistoryQueryValidator : AbstractValidator<GetPaymentHistoryQuery>
{
    private static readonly string[] ValidStatuses = 
    {
        "Pending", "Processing", "Completed", "Failed", "Refunded", "PartiallyRefunded", "Cancelled"
    };

    private static readonly string[] ValidPaymentMethods = 
    {
        "CreditCard", "DebitCard", "Cash", "BankTransfer", "PayPal"
    };

    private static readonly string[] ValidSortFields = 
    {
        "CreatedAt", "Amount", "Status", "ProcessedAt", "PaymentMethod"
    };

    public GetPaymentHistoryQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEqual(Guid.Empty)
            .WithMessage("Customer ID must not be empty")
            .When(x => x.CustomerId.HasValue);

        RuleFor(x => x.BookingId)
            .NotEqual(Guid.Empty)
            .WithMessage("Booking ID must not be empty")
            .When(x => x.BookingId.HasValue);

        RuleFor(x => x.Status)
            .Must(BeValidStatus)
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.PaymentMethod)
            .Must(BeValidPaymentMethod)
            .WithMessage($"Payment method must be one of: {string.Join(", ", ValidPaymentMethods)}")
            .When(x => !string.IsNullOrEmpty(x.PaymentMethod));

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate)
            .WithMessage("From date must be before or equal to to date")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

        RuleFor(x => x.ToDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("To date cannot be in the future")
            .When(x => x.ToDate.HasValue);

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .WithMessage($"Sort by must be one of: {string.Join(", ", ValidSortFields)}")
            .When(x => !string.IsNullOrEmpty(x.SortBy));

        // Validation business logic: ne pas permettre de très grandes plages de dates sans autres filtres
        RuleFor(x => x)
            .Must(HaveReasonableScope)
            .WithMessage("When querying large date ranges, please specify CustomerId or BookingId to limit results")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }

    private static bool BeValidStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        return ValidStatuses.Any(validStatus => 
            string.Equals(validStatus, status, StringComparison.OrdinalIgnoreCase));
    }

    private static bool BeValidPaymentMethod(string? paymentMethod)
    {
        if (string.IsNullOrWhiteSpace(paymentMethod))
            return false;

        return ValidPaymentMethods.Any(validMethod => 
            string.Equals(validMethod, paymentMethod, StringComparison.OrdinalIgnoreCase));
    }

    private static bool BeValidSortField(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return false;

        return ValidSortFields.Any(validField => 
            string.Equals(validField, sortBy, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HaveReasonableScope(GetPaymentHistoryQuery query)
    {
        // Si pas de filtre de date, c'est OK
        if (!query.FromDate.HasValue || !query.ToDate.HasValue)
            return true;

        // Si CustomerId ou BookingId est spécifié, c'est OK
        if (query.CustomerId.HasValue || query.BookingId.HasValue)
            return true;

        // Si la plage de dates est raisonnable (moins de 1 an), c'est OK
        var dateRange = query.ToDate.Value - query.FromDate.Value;
        return dateRange.TotalDays <= 365;
    }
}
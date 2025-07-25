namespace HotelManagement.Application.DTOs.Payment;

public sealed class PaymentHistoryDto
{
    public Guid PaymentId { get; init; }
    public Guid BookingId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string PaymentMethod { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? TransactionReference { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string? FailureReason { get; init; }
    public int ProcessingAttempts { get; init; }
    
    public decimal TotalRefundedAmount { get; init; }
    public decimal RemainingAmount { get; init; }
    public bool HasRefunds { get; init; }
    public int RefundCount { get; init; }
    
    public string? CardType { get; init; }
    public string? CardLast4Digits { get; init; }
    
    public string? BookingReference { get; init; }
    public DateTime? BookingStartDate { get; init; }
    public DateTime? BookingEndDate { get; init; }
}
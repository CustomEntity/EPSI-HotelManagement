namespace HotelManagement.Application.DTOs.Payment;

public sealed class ProcessPaymentResponseDto
{
    public Guid PaymentId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? TransactionReference { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; }
    public string? FailureReason { get; init; }
} 
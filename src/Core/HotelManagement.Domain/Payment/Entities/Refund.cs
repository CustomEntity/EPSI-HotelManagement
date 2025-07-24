using HotelManagement.Domain.Common;
using HotelManagement.Domain.Payment.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Payment.Entities;

public class Refund : Entity
{
    public Guid Id { get; private set; }
    public Money Amount { get; private set; }
    public string Reason { get; private set; }
    public RefundStatus Status { get; private set; }
    public TransactionReference? TransactionReference { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private Refund() 
    {
        Amount = null!;
        Reason = string.Empty;
        Status = null!;
    }

    internal Refund(Money amount, string reason)
    {
        Id = Guid.NewGuid();
        Amount = amount;
        Reason = reason;
        Status = RefundStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    internal void Process(TransactionReference transactionReference)
    {
        if (!Status.CanBeProcessed())
            throw new InvalidOperationException($"Refund cannot be processed in status: {Status}");

        Status = RefundStatus.Processing;
        TransactionReference = transactionReference;
    }

    internal void Complete()
    {
        if (Status != RefundStatus.Processing)
            throw new InvalidOperationException("Refund must be processing to complete");

        Status = RefundStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
    }

    internal void Fail(string reason)
    {
        if (Status != RefundStatus.Processing)
            throw new InvalidOperationException("Refund must be processing to fail");

        Status = RefundStatus.Failed;
        FailureReason = reason;
        ProcessedAt = DateTime.UtcNow;
    }

    internal void Cancel()
    {
        if (!Status.CanBeProcessed())
            throw new InvalidOperationException($"Refund cannot be cancelled in status: {Status}");

        Status = RefundStatus.Cancelled;
    }
}

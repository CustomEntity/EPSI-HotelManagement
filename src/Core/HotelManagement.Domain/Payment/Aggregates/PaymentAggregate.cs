using HotelManagement.Domain.Common;
using HotelManagement.Domain.Payment.Entities;
using HotelManagement.Domain.Payment.Events;
using HotelManagement.Domain.Payment.ValueObjects;
using HotelManagement.Domain.Payment.Services;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Customer.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Payment.Aggregates;

public class Payment : AggregateRoot<PaymentId>
{
    private readonly List<Refund> _refunds = new();

    public BookingId BookingId { get; private set; } = null!;
    public CustomerId CustomerId { get; private set; } = null!;
    public Money Amount { get; private set; } = null!;
    public PaymentMethod Method { get; private set; } = null!;
    public PaymentStatus Status { get; private set; } = null!;
    public TransactionReference? TransactionReference { get; private set; }
    public CreditCard? CreditCard { get; private set; }
    
    public DateTime? ProcessedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public int ProcessingAttempts { get; private set; }
    
    public IReadOnlyCollection<Refund> Refunds => _refunds.AsReadOnly();

    private Payment() { }

    private Payment(
        BookingId bookingId,
        CustomerId customerId,
        Money amount,
        PaymentMethod method)
    {
        Id = PaymentId.New();
        BookingId = bookingId;
        CustomerId = customerId;
        Amount = amount;
        Method = method;
        Status = PaymentStatus.Pending;
        ProcessingAttempts = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Payment> Create(
        BookingId bookingId,
        CustomerId customerId,
        Money amount,
        PaymentMethod method,
        CreditCard? creditCard = null)
    {
        if (amount.Amount <= 0)
            return Result<Payment>.Failure("Payment amount must be greater than zero");

        if (method.RequiresOnlineProcessing() && creditCard == null)
            return Result<Payment>.Failure($"Credit card information is required for {method}");

        var payment = new Payment(bookingId, customerId, amount, method);
        
        if (creditCard != null)
            payment.CreditCard = creditCard;

        return Result<Payment>.Success(payment);
    }

    public async Task<Result> ProcessAsync(IPaymentGateway paymentGateway)
    {
        if (!Status.CanBeProcessed())
            return Result.Failure($"Payment cannot be processed in status: {Status}");

        if (ProcessingAttempts >= 3)
            return Result.Failure("Maximum processing attempts reached");

        try
        {
            ProcessingAttempts++;
            Status = PaymentStatus.Processing;
            ModifiedAt = DateTime.UtcNow;

            Result<TransactionReference> result;

            if (Method.RequiresOnlineProcessing())
            {
                if (CreditCard == null)
                    return Result.Failure("Credit card information is missing");

                result = await paymentGateway.ProcessPaymentAsync(
                    Amount,
                    CreditCard,
                    $"Booking {BookingId}");
            }
            else
            {
                result = Result<TransactionReference>.Success(
                    TransactionReference.Create(
                        $"MANUAL-{Guid.NewGuid():N}",
                        "Manual"));
            }

            if (result.IsFailure)
            {
                return Fail(result.Error);
            }

            return Complete(result.Value);
        }
        catch (Exception ex)
        {
            return Fail($"Payment processing error: {ex.Message}");
        }
    }

    private Result Complete(TransactionReference transactionReference)
    {
        Status = PaymentStatus.Completed;
        TransactionReference = transactionReference;
        ProcessedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentProcessedEvent(
            Id,
            BookingId,
            Amount,
            Method,
            transactionReference));

        return Result.Success();
    }

    private Result Fail(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        ProcessedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentFailedEvent(Id, BookingId, reason));

        return Result.Failure(reason);
    }

    public Result<Refund> RequestRefund(Money amount, string reason)
    {
        if (!Status.CanBeRefunded())
            return Result<Refund>.Failure($"Payment cannot be refunded in status: {Status}");

        var totalRefunded = GetTotalRefundedAmount();
        var remainingAmount = Amount.Amount - totalRefunded.Amount;

        if (amount.Amount > remainingAmount)
            return Result<Refund>.Failure(
                $"Refund amount ({amount}) exceeds remaining amount ({remainingAmount})");

        var refund = new Refund(amount, reason);
        _refunds.Add(refund);

        UpdateRefundStatus();
        ModifiedAt = DateTime.UtcNow;

        return Result<Refund>.Success(refund);
    }

    public async Task<Result> ProcessRefundAsync(
        Guid refundId,
        IPaymentGateway paymentGateway)
    {
        var refund = _refunds.FirstOrDefault(r => r.Id == refundId);
        if (refund == null)
            return Result.Failure("Refund not found");

        if (!refund.Status.CanBeProcessed())
            return Result.Failure($"Refund cannot be processed in status: {refund.Status}");

        if (TransactionReference == null)
            return Result.Failure("Original transaction reference is missing");

        try
        {
            var result = await paymentGateway.ProcessRefundAsync(
                TransactionReference,
                refund.Amount,
                refund.Reason);

            if (result.IsFailure)
            {
                refund.Fail(result.Error);
                return Result.Failure(result.Error);
            }

            refund.Process(result.Value);
            refund.Complete();

            UpdateRefundStatus();
            ModifiedAt = DateTime.UtcNow;

            AddDomainEvent(new RefundProcessedEvent(
                Id,
                refundId,
                refund.Amount,
                result.Value));

            return Result.Success();
        }
        catch (Exception ex)
        {
            refund.Fail($"Refund processing error: {ex.Message}");
            return Result.Failure(ex.Message);
        }
    }

    public Result CancelRefund(Guid refundId)
    {
        var refund = _refunds.FirstOrDefault(r => r.Id == refundId);
        if (refund == null)
            return Result.Failure("Refund not found");

        refund.Cancel();
        UpdateRefundStatus();
        ModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private void UpdateRefundStatus()
    {
        var totalRefunded = GetTotalRefundedAmount();

        if (totalRefunded.Amount == 0)
        {
            if (Status == PaymentStatus.Refunded || Status == PaymentStatus.PartiallyRefunded)
                Status = PaymentStatus.Completed;
        }
        else if (totalRefunded.Amount >= Amount.Amount)
        {
            Status = PaymentStatus.Refunded;
        }
        else
        {
            Status = PaymentStatus.PartiallyRefunded;
        }
    }

    public Money GetTotalRefundedAmount()
    {
        var totalRefunded = _refunds
            .Where(r => r.Status.IsCompleted())
            .Sum(r => r.Amount.Amount);

        return new Money(totalRefunded, Amount.Currency);
    }

    public Money GetRemainingAmount()
    {
        var refunded = GetTotalRefundedAmount();
        return new Money(Amount.Amount - refunded.Amount, Amount.Currency);
    }

    public bool IsFullyRefunded() =>
        Status == PaymentStatus.Refunded;

    public bool HasPendingRefunds() =>
        _refunds.Any(r => r.Status == RefundStatus.Pending || r.Status == RefundStatus.Processing);

    public List<Refund> GetPendingRefunds() =>
        _refunds.Where(r => r.Status == RefundStatus.Pending).ToList();
}

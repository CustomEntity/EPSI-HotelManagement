using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Payment.Commands.ProcessPayment;

public sealed class ProcessPaymentCommand : IRequest<Result<Guid>>
{
    public Guid BookingId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "EUR";
    public string PaymentMethod { get; init; } = string.Empty;
    
    public string? CardNumber { get; init; }
    public string? CardHolderName { get; init; }
    public int? ExpiryMonth { get; init; }
    public int? ExpiryYear { get; init; }
    public string? CVV { get; init; }
    
    public string? Description { get; init; }
}
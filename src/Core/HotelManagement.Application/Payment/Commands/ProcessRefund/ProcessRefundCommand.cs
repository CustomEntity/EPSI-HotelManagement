using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Payment.Commands.ProcessRefund;

public sealed class ProcessRefundCommand : IRequest<Result<Guid>>
{
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "EUR";
    public string Reason { get; init; } = string.Empty;
    public string? Description { get; init; }
}
using HotelManagement.Application.DTOs.Payment;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Payment.Queries.GetPaymentHistory;

public sealed class GetPaymentHistoryQuery : IRequest<Result<List<PaymentHistoryDto>>>
{
    public Guid? CustomerId { get; init; }
    public Guid? BookingId { get; init; }
    public string? Status { get; init; }
    public string? PaymentMethod { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public bool IncludeRefunds { get; init; } = true;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "CreatedAt";
    public bool SortDescending { get; init; } = true;
}
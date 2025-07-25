using HotelManagement.Application.DTOs.Payment;
using HotelManagement.Domain.Booking;
using HotelManagement.Domain.Customer.ValueObjects;
using HotelManagement.Domain.Payment;
using HotelManagement.Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Application.Payment.Queries.GetPaymentHistory;

public sealed class GetPaymentHistoryQueryHandler : IRequestHandler<GetPaymentHistoryQuery, Result<List<PaymentHistoryDto>>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<GetPaymentHistoryQueryHandler> _logger;

    public GetPaymentHistoryQueryHandler(
        IPaymentRepository paymentRepository,
        IBookingRepository bookingRepository,
        ILogger<GetPaymentHistoryQueryHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    public async Task<Result<List<PaymentHistoryDto>>> Handle(
        GetPaymentHistoryQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving payment history with filters: CustomerId={CustomerId}, BookingId={BookingId}, Status={Status}", 
                request.CustomerId, request.BookingId, request.Status);

            List<Domain.Payment.Aggregates.Payment> payments;

            // Récupérer les paiements selon les filtres
            if (request.CustomerId.HasValue)
            {
                var customerId = CustomerId.Create(request.CustomerId.Value);
                payments = await _paymentRepository.GetByCustomerIdAsync(customerId, cancellationToken);
            }
            else if (request.FromDate.HasValue || request.ToDate.HasValue)
            {
                var startDate = request.FromDate ?? DateTime.MinValue;
                var endDate = request.ToDate ?? DateTime.MaxValue;
                payments = await _paymentRepository.GetPaymentsByDateRangeAsync(startDate, endDate, cancellationToken);
            }
            else
            {
                payments = await _paymentRepository.GetAllAsync(cancellationToken);
            }

            // Appliquer les filtres additionnels
            if (request.BookingId.HasValue)
            {
                payments = payments.Where(p => p.BookingId.Value == request.BookingId.Value).ToList();
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                payments = payments.Where(p => p.Status.Value.Equals(request.Status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(request.PaymentMethod))
            {
                payments = payments.Where(p => p.Method.Code.Equals(request.PaymentMethod, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Tri
            payments = ApplySorting(payments, request.SortBy, request.SortDescending);

            // Pagination
            var totalCount = payments.Count;
            var pagedPayments = payments
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Conversion en DTOs
            var paymentHistoryDtos = new List<PaymentHistoryDto>();

            foreach (var payment in pagedPayments)
            {
                // Récupérer les informations de réservation pour le contexte
                var booking = await _bookingRepository.GetByIdAsync(payment.BookingId, cancellationToken);
                
                var dto = new PaymentHistoryDto
                {
                    PaymentId = payment.Id.Value,
                    BookingId = payment.BookingId.Value,
                    CustomerId = payment.CustomerId.Value,
                    Amount = payment.Amount.Amount,
                    Currency = payment.Amount.Currency.Code,
                    PaymentMethod = payment.Method.DisplayName,
                    Status = payment.Status.Value,
                    TransactionReference = payment.TransactionReference?.Value,
                    CreatedAt = payment.CreatedAt,
                    ProcessedAt = payment.ProcessedAt,
                    FailureReason = payment.FailureReason,
                    ProcessingAttempts = payment.ProcessingAttempts,
                    
                    // Informations sur les remboursements
                    TotalRefundedAmount = payment.GetTotalRefundedAmount().Amount,
                    RemainingAmount = payment.GetRemainingAmount().Amount,
                    HasRefunds = payment.Refunds.Any(),
                    RefundCount = payment.Refunds.Count,
                    
                    // Informations sur la carte de crédit (masquées)
                    CardType = payment.CreditCard?.CardType,
                    CardLast4Digits = payment.CreditCard?.Last4Digits,
                    
                    // Informations contextuelles de la réservation
                    BookingReference = booking?.Id.Value.ToString("N")[..8].ToUpperInvariant(),
                    BookingStartDate = booking?.DateRange.StartDate,
                    BookingEndDate = booking?.DateRange.EndDate
                };

                paymentHistoryDtos.Add(dto);
            }

            _logger.LogInformation("Retrieved {PaymentCount} payments out of {TotalCount} total", 
                paymentHistoryDtos.Count, totalCount);

            return Result<List<PaymentHistoryDto>>.Success(paymentHistoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving payment history");
            return Result<List<PaymentHistoryDto>>.Failure("An error occurred while retrieving payment history");
        }
    }

    private static List<Domain.Payment.Aggregates.Payment> ApplySorting(
        List<Domain.Payment.Aggregates.Payment> payments, 
        string sortBy, 
        bool sortDescending)
    {
        var query = payments.AsQueryable();

        query = sortBy.ToLowerInvariant() switch
        {
            "amount" => sortDescending 
                ? query.OrderByDescending(p => p.Amount.Amount) 
                : query.OrderBy(p => p.Amount.Amount),
            "status" => sortDescending 
                ? query.OrderByDescending(p => p.Status.Value) 
                : query.OrderBy(p => p.Status.Value),
            "processedat" => sortDescending 
                ? query.OrderByDescending(p => p.ProcessedAt) 
                : query.OrderBy(p => p.ProcessedAt),
            "paymentmethod" => sortDescending 
                ? query.OrderByDescending(p => p.Method.DisplayName) 
                : query.OrderBy(p => p.Method.DisplayName),
            _ => sortDescending 
                ? query.OrderByDescending(p => p.CreatedAt) 
                : query.OrderBy(p => p.CreatedAt) // Default: CreatedAt
        };

        return query.ToList();
    }
}
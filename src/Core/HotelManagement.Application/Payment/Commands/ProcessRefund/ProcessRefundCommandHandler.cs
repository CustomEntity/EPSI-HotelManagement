using HotelManagement.Domain.Common;
using HotelManagement.Domain.Payment;
using HotelManagement.Domain.Payment.Services;
using HotelManagement.Domain.Payment.ValueObjects;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Payment.Commands.ProcessRefund;

public sealed class ProcessRefundCommandHandler : IRequestHandler<ProcessRefundCommand, Result<Guid>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessRefundCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(ProcessRefundCommand request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            return Result<Guid>.Failure("Refund amount must be greater than zero");

        if (string.IsNullOrWhiteSpace(request.Reason))
            return Result<Guid>.Failure("Refund reason is required");
            
        var refundPaymentId = PaymentId.Create(request.PaymentId);
        
        var currencyResult = Currency.Create(request.Currency);
        if (currencyResult.IsFailure)
            return Result<Guid>.Failure(currencyResult.Error);

        var moneyResult = Money.Create(request.Amount, currencyResult.Value);
        if (moneyResult.IsFailure)
            return Result<Guid>.Failure(moneyResult.Error);

        var payment = await _paymentRepository.GetByIdWithRefundsAsync(refundPaymentId, cancellationToken);
        if (payment == null)
            return Result<Guid>.Failure("Payment not found");

        if (!payment.Status.CanBeRefunded())
            return Result<Guid>.Failure($"Payment cannot be refunded in status: {payment.Status}");

        var refundRequestResult = payment.RequestRefund(moneyResult.Value, request.Reason);
        if (refundRequestResult.IsFailure)
            return Result<Guid>.Failure(refundRequestResult.Error);

        var refund = refundRequestResult.Value;

        var processResult = await payment.ProcessRefundAsync(refund.Id, _paymentGateway);
        if (processResult.IsFailure)
        {
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Failure(processResult.Error);
        }

        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(refund.Id);
    }
}
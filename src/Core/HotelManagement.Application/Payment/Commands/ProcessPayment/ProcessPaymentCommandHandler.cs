using HotelManagement.Domain.Booking;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Customer;
using HotelManagement.Domain.Customer.ValueObjects;
using HotelManagement.Domain.Payment;
using HotelManagement.Domain.Payment.Services;
using HotelManagement.Domain.Payment.ValueObjects;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Payment.Commands.ProcessPayment;

public sealed class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<Guid>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IBookingRepository bookingRepository,
        ICustomerRepository customerRepository,
        IPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
        _customerRepository = customerRepository;
        _paymentGateway = paymentGateway;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var bookingId = BookingId.Create(request.BookingId);
        var customerId = CustomerId.Create(request.CustomerId);

        var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
        if (booking == null)
            return Result<Guid>.Failure("Booking not found");

        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer == null)
            return Result<Guid>.Failure("Customer not found");

        if (booking.CustomerId.Value != customerId.Value)
            return Result<Guid>.Failure("Customer does not match the booking");

        var existingPayment = await _paymentRepository.GetByBookingIdAsync(bookingId, cancellationToken);
        if (existingPayment?.Status.IsSuccessful() == true)
            return Result<Guid>.Failure("Payment already exists for this booking");

        var moneyResult = Money.Create(request.Amount, request.Currency);
        if (moneyResult.IsFailure)
            return Result<Guid>.Failure(moneyResult.Error);

        var paymentMethodResult = CreatePaymentMethod(request.PaymentMethod);
        if (paymentMethodResult.IsFailure)
            return Result<Guid>.Failure(paymentMethodResult.Error);

        CreditCard? creditCard = null;
        if (paymentMethodResult.Value.RequiresOnlineProcessing())
        {
            var creditCardResult = CreateCreditCard(request);
            if (creditCardResult.IsFailure)
                return Result<Guid>.Failure(creditCardResult.Error);
            creditCard = creditCardResult.Value;
        }

        var paymentResult = Domain.Payment.Aggregates.Payment.Create(
            bookingId,
            customerId,
            moneyResult.Value,
            paymentMethodResult.Value,
            creditCard);

        if (paymentResult.IsFailure)
            return Result<Guid>.Failure(paymentResult.Error);

        var payment = paymentResult.Value;

        var processResult = await payment.ProcessAsync(_paymentGateway);
        if (processResult.IsFailure)
        {
            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Failure(processResult.Error);
        }

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(payment.Id.Value);
    }

    private static Result<PaymentMethod> CreatePaymentMethod(string paymentMethodCode)
    {
        try
        {
            var method = PaymentMethod.Create(paymentMethodCode);
            return Result<PaymentMethod>.Success(method);
        }
        catch (ArgumentException ex)
        {
            return Result<PaymentMethod>.Failure(ex.Message);
        }
    }

    private static Result<CreditCard> CreateCreditCard(ProcessPaymentCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.CardNumber) ||
            string.IsNullOrWhiteSpace(request.CardHolderName) ||
            !request.ExpiryMonth.HasValue ||
            !request.ExpiryYear.HasValue ||
            string.IsNullOrWhiteSpace(request.CVV))
        {
            return Result<CreditCard>.Failure("Credit card information is incomplete");
        }

        try
        {
            var creditCard = CreditCard.Create(
                request.CardNumber,
                request.CardHolderName,
                request.ExpiryMonth.Value,
                request.ExpiryYear.Value,
                request.CVV);
            
            return CreditCard.Create(
                request.CardNumber,
                request.CardHolderName,
                request.ExpiryMonth.Value,
                request.ExpiryYear.Value,
                request.CVV);
        }
        catch (ArgumentException ex)
        {
            return Result<CreditCard>.Failure(ex.Message);
        }
    }
}
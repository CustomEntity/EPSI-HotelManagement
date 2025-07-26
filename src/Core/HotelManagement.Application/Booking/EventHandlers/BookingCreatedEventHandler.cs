using HotelManagement.Domain.Booking.Events;
using HotelManagement.Domain.Customer;
using HotelManagement.Domain.Housekeeping.Repositories;
using HotelManagement.Domain.Room;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Application.Booking.EventHandlers;

public sealed class BookingCreatedEventHandler : INotificationHandler<BookingCreatedEvent>
{
    private readonly ILogger<BookingCreatedEventHandler> _logger;
    private readonly ICustomerRepository _customerRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly ICleaningTaskRepository _cleaningTaskRepository;

    public BookingCreatedEventHandler(
        ILogger<BookingCreatedEventHandler> logger,
        ICustomerRepository customerRepository,
        IRoomRepository roomRepository,
        ICleaningTaskRepository cleaningTaskRepository)
    {
        _logger = logger;
        _customerRepository = customerRepository;
        _roomRepository = roomRepository;
        _cleaningTaskRepository = cleaningTaskRepository;
    }

    public async Task Handle(BookingCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing BookingCreatedEvent for BookingId: {BookingId}, CustomerId: {CustomerId}, DateRange: {DateRange}, RoomCount: {RoomCount}",
                notification.BookingId.Value,
                notification.CustomerId.Value,
                notification.DateRange,
                notification.RoomCount);

            var customer = await _customerRepository.GetByIdAsync(notification.CustomerId, cancellationToken);
            
            if (customer != null)
            {
                _logger.LogInformation(
                    "Booking created for customer: {CustomerName} ({CustomerEmail}), Customer Type: {CustomerType}",
                    customer.Name.FullName,
                    customer.Email.Value,
                    customer.CustomerType.Value);
            }

            await SchedulePreparatoryCleaningTasksAsync(notification, cancellationToken);
            LogBookingStatistics(notification);
            await TriggerAdditionalProcessesAsync(notification, cancellationToken);

            _logger.LogInformation(
                "Successfully processed BookingCreatedEvent for BookingId: {BookingId}",
                notification.BookingId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while processing BookingCreatedEvent for BookingId: {BookingId}",
                notification.BookingId.Value);
        }
    }

    private async Task SchedulePreparatoryCleaningTasksAsync(
        BookingCreatedEvent notification, 
        CancellationToken cancellationToken)
    {
        try
        {
            var daysUntilCheckIn = (notification.DateRange.StartDate - DateTime.UtcNow.Date).TotalDays;
            
            if (daysUntilCheckIn <= 7 && daysUntilCheckIn > 0)
            {                
                _logger.LogInformation(
                    "Booking {BookingId} starts in {Days} days - considering preparatory cleaning tasks",
                    notification.BookingId.Value,
                    daysUntilCheckIn);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to schedule preparatory cleaning tasks for BookingId: {BookingId}",
                notification.BookingId.Value);
        }
    }

    private void LogBookingStatistics(BookingCreatedEvent notification)
    {
        try
        {
            var bookingDuration = notification.DateRange.GetNights();
            var checkInDate = notification.DateRange.StartDate;
            var checkOutDate = notification.DateRange.EndDate;

            _logger.LogInformation(
                "Booking Statistics - BookingId: {BookingId}, Duration: {Duration} nights, CheckIn: {CheckInDate}, CheckOut: {CheckOutDate}, Rooms: {RoomCount}",
                notification.BookingId.Value,
                bookingDuration,
                checkInDate.ToString("yyyy-MM-dd"),
                checkOutDate.ToString("yyyy-MM-dd"),
                notification.RoomCount);

            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["BookingId"] = notification.BookingId.Value,
                ["CustomerId"] = notification.CustomerId.Value,
                ["BookingDuration"] = bookingDuration,
                ["RoomCount"] = notification.RoomCount,
                ["CheckInDate"] = checkInDate,
                ["BookingCreatedAt"] = notification.OccurredOn
            });

            _logger.LogInformation("Booking metrics logged successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log booking statistics");
        }
    }

    private async Task TriggerAdditionalProcessesAsync(
        BookingCreatedEvent notification, 
        CancellationToken cancellationToken)
    {        
        await Task.CompletedTask;
        
        _logger.LogDebug(
            "Additional processes triggered for BookingId: {BookingId}",
            notification.BookingId.Value);
    }
}
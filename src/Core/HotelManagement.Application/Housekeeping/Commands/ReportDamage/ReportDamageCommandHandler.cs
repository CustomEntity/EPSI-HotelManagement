using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Housekeeping.Repositories;
using HotelManagement.Domain.Housekeeping.Services;
using HotelManagement.Domain.Housekeeping.ValueObjects;
using HotelManagement.Domain.Room;
using HotelManagement.Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Application.Housekeeping.Commands.ReportDamage;

public sealed class ReportDamageCommandHandler : IRequestHandler<ReportDamageCommand, Result>
{
    private readonly ICleaningTaskRepository _cleaningTaskRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IHousekeepingNotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReportDamageCommandHandler> _logger;

    public ReportDamageCommandHandler(
        ICleaningTaskRepository cleaningTaskRepository,
        IRoomRepository roomRepository,
        IHousekeepingNotificationService notificationService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<ReportDamageCommandHandler> logger)
    {
        _cleaningTaskRepository = cleaningTaskRepository;
        _roomRepository = roomRepository;
        _notificationService = notificationService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ReportDamageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Reporting damage for cleaning task {CleaningTaskId}",
                request.CleaningTaskId);

            var cleaningTaskId = CleaningTaskId.Create(request.CleaningTaskId);
            var cleaningTask = await _cleaningTaskRepository.GetByIdAsync(cleaningTaskId, cancellationToken);

            if (cleaningTask == null)
            {
                _logger.LogWarning(
                    "Cleaning task not found with ID: {CleaningTaskId}",
                    request.CleaningTaskId);
                return Result.Failure("Cleaning task not found");
            }

            if (cleaningTask.Status == CleaningStatus.Cancelled)
            {
                _logger.LogWarning(
                    "Cannot report damage on cancelled cleaning task {CleaningTaskId}",
                    request.CleaningTaskId);
                return Result.Failure("Cannot report damage on a cancelled cleaning task");
            }

            var reportedBy = DetermineReporter(request.ReportedBy);

            var room = await _roomRepository.GetByIdAsync(cleaningTask.RoomId, cancellationToken);
            if (room == null)
            {
                _logger.LogWarning(
                    "Room not found with ID: {RoomId} for cleaning task {CleaningTaskId}",
                    cleaningTask.RoomId.Value,
                    request.CleaningTaskId);
                return Result.Failure("Associated room not found");
            }

            var reportResult = cleaningTask.ReportDamage(
                request.DamageDescription,
                request.EstimatedRepairCost);

            if (reportResult.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to report damage for cleaning task {CleaningTaskId}: {Error}",
                    request.CleaningTaskId,
                    reportResult.Error);
                return Result.Failure(reportResult.Error);
            }

            if (request.EstimatedRepairCost.HasValue && request.EstimatedRepairCost.Value > 500)
            {
                _logger.LogInformation(
                    "High damage cost reported ({Cost:C}), marking room {RoomId} for maintenance",
                    request.EstimatedRepairCost.Value,
                    room.Id.Value);
                    
                room.StartMaintenance();
                await _roomRepository.UpdateAsync(room, cancellationToken);
            }

            await _cleaningTaskRepository.UpdateAsync(cleaningTask, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                await _notificationService.NotifyDamageReportedAsync(
                    cleaningTask.Id,
                    cleaningTask.RoomId,
                    request.DamageDescription);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to send damage notification for cleaning task {CleaningTaskId}",
                    request.CleaningTaskId);
            }

            _logger.LogInformation(
                "Successfully reported damage for cleaning task {CleaningTaskId} in room {RoomId}. " +
                "Estimated cost: {EstimatedCost:C}, Reported by: {ReportedBy}",
                cleaningTask.Id.Value,
                cleaningTask.RoomId.Value,
                request.EstimatedRepairCost,
                reportedBy);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while reporting damage for cleaning task {CleaningTaskId}",
                request.CleaningTaskId);
              return Result.Failure("An error occurred while reporting the damage");
        }
    }

    private string DetermineReporter(string? requestedReporter)
    {
        if (!string.IsNullOrWhiteSpace(requestedReporter))
            return requestedReporter.Trim();

        if (_currentUserService.IsAuthenticated && !string.IsNullOrWhiteSpace(_currentUserService.Email))
            return _currentUserService.Email;

        return "System";
    }
}
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Housekeeping.Commands.CompleteCleaningTask;

public sealed class CompleteCleaningTaskCommand : IRequest<Result>
{
    public Guid CleaningTaskId { get; init; }
    public string? CompletionNotes { get; init; }

    public CompleteCleaningTaskCommand(Guid cleaningTaskId, string? completionNotes = null)
    {
        CleaningTaskId = cleaningTaskId;
        CompletionNotes = completionNotes;
    }

    public CompleteCleaningTaskCommand() { }
}
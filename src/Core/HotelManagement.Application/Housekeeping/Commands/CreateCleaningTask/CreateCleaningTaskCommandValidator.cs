using FluentValidation;

namespace HotelManagement.Application.Housekeeping.Commands.CreateCleaningTask;

public sealed class CreateCleaningTaskCommandValidator : AbstractValidator<CreateCleaningTaskCommand>
{
    private static readonly string[] ValidPriorities = 
    {
        "Low", "Normal", "High", "Urgent"
    };

    public CreateCleaningTaskCommandValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty()
            .WithMessage("Room ID is required");

        RuleFor(x => x.Priority)
            .NotEmpty()
            .WithMessage("Priority is required")
            .Must(BeValidPriority)
            .WithMessage($"Priority must be one of: {string.Join(", ", ValidPriorities)}");

        RuleFor(x => x.ScheduledFor)
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Scheduled time cannot be in the past")
            .When(x => x.ScheduledFor.HasValue);

        RuleFor(x => x.ScheduledFor)
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(1))
            .WithMessage("Scheduled time cannot be more than one year in the future")
            .When(x => x.ScheduledFor.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.AssignedTo)
            .MaximumLength(200)
            .WithMessage("Assigned to cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.AssignedTo));
    }

    private static bool BeValidPriority(string priority)
    {
        if (string.IsNullOrWhiteSpace(priority))
            return false;

        return ValidPriorities.Any(validPriority => 
            string.Equals(validPriority, priority, StringComparison.OrdinalIgnoreCase));
    }
}
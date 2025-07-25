using FluentValidation;

namespace HotelManagement.Application.Housekeeping.Queries.GetPriorityCleaningTasks;

public sealed class GetPriorityCleaningTasksQueryValidator : AbstractValidator<GetPriorityCleaningTasksQuery>
{
    private static readonly string[] ValidPriorities = 
    {
        "Low", "Normal", "High", "Urgent"
    };

    private static readonly string[] ValidStatuses = 
    {
        "Pending", "InProgress", "Completed", "Cancelled"
    };

    private static readonly string[] ValidSortFields = 
    {
        "Priority", "ScheduledFor", "Status", "Overdue"
    };

    public GetPriorityCleaningTasksQueryValidator()
    {
        RuleFor(x => x.MinimumPriority)
            .NotEmpty()
            .WithMessage("Minimum priority is required")
            .Must(BeValidPriority)
            .WithMessage($"Minimum priority must be one of: {string.Join(", ", ValidPriorities)}");

        RuleFor(x => x.Status)
            .Must(BeValidStatus)
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.RoomId)
            .NotEqual(Guid.Empty)
            .WithMessage("Room ID must not be empty")
            .When(x => x.RoomId.HasValue);

        RuleFor(x => x.AssignedTo)
            .MaximumLength(200)
            .WithMessage("Assigned to cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.AssignedTo));

        RuleFor(x => x.DueWithinHours)
            .GreaterThan(0)
            .WithMessage("Due within hours must be greater than 0")
            .LessThanOrEqualTo(168)
            .WithMessage("Due within hours cannot exceed 168 hours (1 week)")
            .When(x => x.DueWithinHours.HasValue);

        RuleFor(x => x.MaxResults)
            .InclusiveBetween(1, 200)
            .WithMessage("Max results must be between 1 and 200");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .WithMessage($"Sort by must be one of: {string.Join(", ", ValidSortFields)}")
            .When(x => !string.IsNullOrEmpty(x.SortBy));

        RuleFor(x => x)
            .Must(x => !(x.OnlyUnassigned && !string.IsNullOrEmpty(x.AssignedTo)))
            .WithMessage("Cannot specify both OnlyUnassigned=true and AssignedTo filter");

        RuleFor(x => x)
            .Must(x => !(x.OnlyOverdue && x.DueWithinHours.HasValue))
            .WithMessage("Cannot specify both OnlyOverdue=true and DueWithinHours filter");

        RuleFor(x => x)
            .Must(HaveReasonableFiltering)
            .WithMessage("For performance reasons, please specify at least one filter (RoomId, AssignedTo, OnlyOverdue, OnlyUnassigned, or DueWithinHours)")
            .When(x => x.MaxResults > 50);
    }

    private static bool BeValidPriority(string priority)
    {
        if (string.IsNullOrWhiteSpace(priority))
            return false;

        return ValidPriorities.Any(validPriority => 
            string.Equals(validPriority, priority, StringComparison.OrdinalIgnoreCase));
    }

    private static bool BeValidStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        return ValidStatuses.Any(validStatus => 
            string.Equals(validStatus, status, StringComparison.OrdinalIgnoreCase));
    }

    private static bool BeValidSortField(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return false;

        return ValidSortFields.Any(validField => 
            string.Equals(validField, sortBy, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HaveReasonableFiltering(GetPriorityCleaningTasksQuery query)
    {
        if (query.MaxResults <= 50)
            return true;

        return query.RoomId.HasValue ||
               !string.IsNullOrEmpty(query.AssignedTo) ||
               query.OnlyOverdue ||
               query.OnlyUnassigned ||
               query.DueWithinHours.HasValue;
    }
}
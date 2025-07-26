using FluentValidation;

namespace HotelManagement.Application.Housekeeping.Queries.GetCleaningTasks;

public sealed class GetCleaningTasksQueryValidator : AbstractValidator<GetCleaningTasksQuery>
{
    private static readonly string[] ValidStatuses = 
    {
        "Pending", "InProgress", "Completed", "Cancelled"
    };

    private static readonly string[] ValidPriorities = 
    {
        "Low", "Normal", "High", "Urgent"
    };

    private static readonly string[] ValidSortFields = 
    {
        "Priority", "Status", "ScheduledFor", "CreatedAt", "AssignedTo", "RoomId"
    };

    public GetCleaningTasksQueryValidator()
    {
        RuleFor(x => x.Status)
            .Must(BeValidStatus)
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Priority)
            .Must(BeValidPriority)
            .WithMessage($"Priority must be one of: {string.Join(", ", ValidPriorities)}")
            .When(x => !string.IsNullOrEmpty(x.Priority));

        RuleFor(x => x.RoomId)
            .NotEqual(Guid.Empty)
            .WithMessage("Room ID must not be empty")
            .When(x => x.RoomId.HasValue);

        RuleFor(x => x.AssignedTo)
            .MaximumLength(200)
            .WithMessage("Assigned to cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.AssignedTo));

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate)
            .WithMessage("From date must be before or equal to to date")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

        RuleFor(x => x.ToDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("To date cannot be more than one day in the future")
            .When(x => x.ToDate.HasValue);

        RuleFor(x => x.ScheduledDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(1))
            .WithMessage("Scheduled date cannot be more than one year in the future")
            .When(x => x.ScheduledDate.HasValue);

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .WithMessage($"Sort by must be one of: {string.Join(", ", ValidSortFields)}")
            .When(x => !string.IsNullOrEmpty(x.SortBy));

        RuleFor(x => x)
            .Must(HaveReasonableScope)
            .WithMessage("When querying large date ranges, please specify additional filters to limit results")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }

    private static bool BeValidStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        return ValidStatuses.Any(validStatus => 
            string.Equals(validStatus, status, StringComparison.OrdinalIgnoreCase));
    }

    private static bool BeValidPriority(string? priority)
    {
        if (string.IsNullOrWhiteSpace(priority))
            return false;

        return ValidPriorities.Any(validPriority => 
            string.Equals(validPriority, priority, StringComparison.OrdinalIgnoreCase));
    }

    private static bool BeValidSortField(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return false;

        return ValidSortFields.Any(validField => 
            string.Equals(validField, sortBy, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HaveReasonableScope(GetCleaningTasksQuery query)
    {
        if (!query.FromDate.HasValue || !query.ToDate.HasValue)
            return true;

        if (query.RoomId.HasValue || 
            !string.IsNullOrEmpty(query.Status) || 
            !string.IsNullOrEmpty(query.Priority) ||
            !string.IsNullOrEmpty(query.AssignedTo))
            return true;

        var dateRange = query.ToDate.Value - query.FromDate.Value;
        return dateRange.TotalDays <= 90;
    }
}
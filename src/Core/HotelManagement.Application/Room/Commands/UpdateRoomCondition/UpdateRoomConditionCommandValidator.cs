using FluentValidation;

namespace HotelManagement.Application.Room.Commands.UpdateRoomCondition;

public sealed class UpdateRoomConditionCommandValidator : AbstractValidator<UpdateRoomConditionCommand>
{
    private static readonly string[] ValidConditions = 
    {
        "New", "Refurbished", "NeedsRefurbishment", "Good", "Damaged"
    };

    public UpdateRoomConditionCommandValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty()
            .WithMessage("Room ID is required");

        RuleFor(x => x.Condition)
            .NotEmpty()
            .WithMessage("Room condition is required")
            .Must(BeValidCondition)
            .WithMessage($"Invalid room condition. Valid values are: {string.Join(", ", ValidConditions)}");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.ReportedBy)
            .MaximumLength(200)
            .WithMessage("Reported by cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.ReportedBy));
    }

    private bool BeValidCondition(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return false;

        var normalizedCondition = condition.Trim();
        return ValidConditions.Any(validCondition => 
            string.Equals(validCondition, normalizedCondition, StringComparison.OrdinalIgnoreCase));
    }
}
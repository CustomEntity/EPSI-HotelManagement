using FluentValidation;

namespace HotelManagement.Application.Housekeeping.Commands.ReportDamage;

public sealed class ReportDamageCommandValidator : AbstractValidator<ReportDamageCommand>
{
    public ReportDamageCommandValidator()
    {
        RuleFor(x => x.CleaningTaskId)
            .NotEmpty()
            .WithMessage("Cleaning task ID is required");

        RuleFor(x => x.DamageDescription)
            .NotEmpty()
            .WithMessage("Damage description is required")
            .MinimumLength(10)
            .WithMessage("Damage description must be at least 10 characters long")
            .MaximumLength(2000)
            .WithMessage("Damage description cannot exceed 2000 characters");

        RuleFor(x => x.EstimatedRepairCost)
            .GreaterThan(0)
            .WithMessage("Estimated repair cost must be greater than zero")
            .LessThanOrEqualTo(999999.99m)
            .WithMessage("Estimated repair cost cannot exceed 999,999.99")
            .When(x => x.EstimatedRepairCost.HasValue);

        RuleFor(x => x.ReportedBy)
            .NotEmpty()
            .WithMessage("Reporter name is required")
            .MaximumLength(200)
            .WithMessage("Reporter name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.ReportedBy));

        RuleFor(x => x.DamageDescription)
            .Must(NotContainInvalidCharacters)
            .WithMessage("Damage description contains invalid characters");
    }

    private static bool NotContainInvalidCharacters(string description)
    {
        if (string.IsNullOrEmpty(description))
            return true;    

        return !description.Any(c => char.IsControl(c) && c != '\r' && c != '\n' && c != '\t');
    }
}
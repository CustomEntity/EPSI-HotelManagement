using FluentValidation;

namespace HotelManagement.Application.Housekeeping.Commands.CompleteCleaningTask;

public sealed class CompleteCleaningTaskCommandValidator : AbstractValidator<CompleteCleaningTaskCommand>
{
    public CompleteCleaningTaskCommandValidator()
    {
        RuleFor(x => x.CleaningTaskId)
            .NotEmpty()
            .WithMessage("Cleaning task ID is required");

        RuleFor(x => x.CompletionNotes)
            .MaximumLength(1000)
            .WithMessage("Completion notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.CompletionNotes));

        RuleFor(x => x.CompletionNotes)
            .Must(NotContainInvalidCharacters)
            .WithMessage("Completion notes contain invalid characters")
            .When(x => !string.IsNullOrEmpty(x.CompletionNotes));
    }

    private static bool NotContainInvalidCharacters(string? notes)
    {
        if (string.IsNullOrEmpty(notes))
            return true;

        return !notes.Any(c => char.IsControl(c) && c != '\r' && c != '\n' && c != '\t');
    }
}
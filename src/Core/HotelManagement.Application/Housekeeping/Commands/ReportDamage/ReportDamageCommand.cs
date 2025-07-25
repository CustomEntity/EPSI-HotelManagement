using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Housekeeping.Commands.ReportDamage;

public sealed class ReportDamageCommand : IRequest<Result>
{
    public Guid CleaningTaskId { get; init; }
    public string DamageDescription { get; init; } = string.Empty;
    public decimal? EstimatedRepairCost { get; init; }
    public string? ReportedBy { get; init; }

    public ReportDamageCommand(
        Guid cleaningTaskId, 
        string damageDescription, 
        decimal? estimatedRepairCost = null,
        string? reportedBy = null)
    {
        CleaningTaskId = cleaningTaskId;
        DamageDescription = damageDescription;
        EstimatedRepairCost = estimatedRepairCost;
        ReportedBy = reportedBy;
    }

    public ReportDamageCommand() { }
}
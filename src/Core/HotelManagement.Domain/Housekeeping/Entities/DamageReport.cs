using HotelManagement.Domain.Common;
using HotelManagement.Domain.Housekeeping.ValueObjects;

namespace HotelManagement.Domain.Housekeeping.Entities;

public class DamageReport : Entity
{
    public Guid Id { get; private set; }
    public CleaningTaskId CleaningTaskId { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal? EstimatedRepairCost { get; private set; }
    public DateTime ReportedAt { get; private set; }
    public string ReportedBy { get; private set; } = null!;
    public bool IsRepaired { get; private set; }
    public DateTime? RepairedAt { get; private set; }
    public decimal? ActualRepairCost { get; private set; }
    public string? RepairNotes { get; private set; }

    private DamageReport() { }

    public DamageReport(
        CleaningTaskId cleaningTaskId,
        string description,
        string reportedBy,
        decimal? estimatedRepairCost = null)
    {
        Id = Guid.NewGuid();
        CleaningTaskId = cleaningTaskId;
        Description = description;
        ReportedBy = reportedBy;
        EstimatedRepairCost = estimatedRepairCost;
        ReportedAt = DateTime.UtcNow;
        IsRepaired = false;
    }

    public void MarkAsRepaired(decimal? actualCost = null, string? notes = null)
    {
        IsRepaired = true;
        RepairedAt = DateTime.UtcNow;
        ActualRepairCost = actualCost;
        RepairNotes = notes;
    }
}
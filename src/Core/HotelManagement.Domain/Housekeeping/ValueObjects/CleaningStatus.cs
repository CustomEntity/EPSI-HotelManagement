using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Housekeeping.ValueObjects;

public sealed class CleaningStatus : ValueObject
{
    public static readonly CleaningStatus Pending = new("Pending");
    public static readonly CleaningStatus InProgress = new("InProgress");
    public static readonly CleaningStatus Completed = new("Completed");
    public static readonly CleaningStatus Cancelled = new("Cancelled");

    private static readonly CleaningStatus[] _values = 
    {
        Pending, InProgress, Completed, Cancelled
    };

    public string Value { get; }

    private CleaningStatus(string value)
    {
        Value = value;
    }

    public static CleaningStatus? FromString(string value)
    {
        return _values.FirstOrDefault(s => s.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
    }

    public static CleaningStatus FromStringOrThrow(string value)
    {
        var status = FromString(value);
        if (status is null)
        {
            throw new ArgumentException($"Invalid cleaning status: {value}");
        }
        return status;
    }

    public bool IsFinal => this == Completed || this == Cancelled;
    public bool IsActive => this == Pending || this == InProgress;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(CleaningStatus status) => status.Value;
}
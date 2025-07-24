using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Housekeeping.ValueObjects;

public sealed class CleaningTaskId : ValueObject
{
    public Guid Value { get; }

    private CleaningTaskId(Guid value)
    {
        Value = value;
    }

    public static CleaningTaskId CreateUnique() => new(Guid.NewGuid());

    public static CleaningTaskId Create(Guid value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(CleaningTaskId cleaningTaskId) => cleaningTaskId.Value;
}

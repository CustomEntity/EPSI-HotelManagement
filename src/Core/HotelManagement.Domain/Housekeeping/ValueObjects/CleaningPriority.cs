using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Housekeeping.ValueObjects;

public sealed class CleaningPriority : ValueObject
{
    public static readonly CleaningPriority Low = new("Low", 1);
    public static readonly CleaningPriority Normal = new("Normal", 2);
    public static readonly CleaningPriority High = new("High", 3);
    public static readonly CleaningPriority Urgent = new("Urgent", 4);

    private static readonly CleaningPriority[] _values = 
    {
        Low, Normal, High, Urgent
    };

    public string Name { get; }
    public int Level { get; }

    private CleaningPriority(string name, int level)
    {
        Name = name;
        Level = level;
    }

    public static CleaningPriority? FromString(string name)
    {
        return _values.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public static CleaningPriority FromStringOrThrow(string name)
    {
        var priority = FromString(name);
        if (priority is null)
        {
            throw new ArgumentException($"Invalid cleaning priority: {name}");
        }
        return priority;
    }

    public static CleaningPriority FromLevel(int level)
    {
        return _values.FirstOrDefault(p => p.Level == level) ?? Normal;
    }

    public bool IsHigherThan(CleaningPriority other) => Level > other.Level;
    public bool IsLowerThan(CleaningPriority other) => Level < other.Level;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Level;
    }

    public override string ToString() => Name;

    public static implicit operator string(CleaningPriority priority) => priority.Name;
    public static implicit operator int(CleaningPriority priority) => priority.Level;
}
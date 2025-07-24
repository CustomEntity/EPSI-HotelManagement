using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Customer.ValueObjects;

public sealed class PersonalName : ValueObject
{
    public string FirstName { get; }
    public string LastName { get; }
    public string FullName => $"{FirstName} {LastName}";

    private PersonalName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static PersonalName Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

        firstName = firstName.Trim();
        lastName = lastName.Trim();

        if (firstName.Length > 100)
            throw new ArgumentException("First name cannot exceed 100 characters", nameof(firstName));

        if (lastName.Length > 100)
            throw new ArgumentException("Last name cannot exceed 100 characters", nameof(lastName));

        return new PersonalName(firstName, lastName);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }

    public override string ToString() => FullName;
}
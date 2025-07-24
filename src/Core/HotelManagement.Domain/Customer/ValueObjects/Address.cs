using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Customer.ValueObjects;

public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Country { get; }
    public string? State { get; }

    private Address(string street, string city, string postalCode, string country, string? state = null)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
        State = state;
    }

    public static Address Create(string street, string city, string postalCode, string country, string? state = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be null or empty", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty", nameof(city));

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be null or empty", nameof(postalCode));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be null or empty", nameof(country));

        return new Address(
            street.Trim(),
            city.Trim(),
            postalCode.Trim(),
            country.Trim(),
            state?.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return Country;
        if (State != null) yield return State;
    }

    public override string ToString() => 
        $"{Street}, {City} {PostalCode}, {Country}" + (State != null ? $", {State}" : "");
}
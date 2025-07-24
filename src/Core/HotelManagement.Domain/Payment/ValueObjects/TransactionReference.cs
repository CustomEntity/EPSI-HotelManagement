using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Payment.ValueObjects;

public sealed class TransactionReference : ValueObject
{
    public string Value { get; }
    public string Provider { get; }
    public DateTime CreatedAt { get; }

    private TransactionReference(string value, string provider)
    {
        Value = value;
        Provider = provider;
        CreatedAt = DateTime.UtcNow;
    }

    public static TransactionReference Create(string value, string provider)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Transaction reference cannot be null or empty", nameof(value));

        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("Provider cannot be null or empty", nameof(provider));

        return new TransactionReference(value.Trim(), provider.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Provider;
    }

    public override string ToString() => $"{Provider}:{Value}";
}
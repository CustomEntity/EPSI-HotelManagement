using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Payment.ValueObjects;

public sealed class PaymentMethod : ValueObject
{
    public static readonly PaymentMethod CreditCard = new("CreditCard", "Credit Card");
    public static readonly PaymentMethod DebitCard = new("DebitCard", "Debit Card");
    public static readonly PaymentMethod Cash = new("Cash", "Cash");
    public static readonly PaymentMethod BankTransfer = new("BankTransfer", "Bank Transfer");
    public static readonly PaymentMethod PayPal = new("PayPal", "PayPal");

    public string Code { get; }
    public string DisplayName { get; }

    private PaymentMethod(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    public static PaymentMethod Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Payment method code cannot be null or empty", nameof(code));

        return code.Trim() switch
        {
            "CreditCard" => CreditCard,
            "DebitCard" => DebitCard,
            "Cash" => Cash,
            "BankTransfer" => BankTransfer,
            "PayPal" => PayPal,
            _ => throw new ArgumentException($"Invalid payment method: {code}", nameof(code))
        };
    }

    public bool RequiresOnlineProcessing() =>
        this == CreditCard || this == DebitCard || this == PayPal;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }

    public override string ToString() => DisplayName;
}
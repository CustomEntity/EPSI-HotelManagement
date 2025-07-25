using HotelManagement.Domain.Common;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Shared;

public class Currency : ValueObject
{
    public static readonly Currency EUR = new("EUR", "€");
    public static readonly Currency USD = new("USD", "$");
    public static readonly Currency GBP = new("GBP", "£");
    public static readonly Currency JPY = new("JPY", "¥");
    public static readonly Currency CHF = new("CHF", "₣");

    public string Code { get; }
    public string Symbol { get; }

    private Currency(string code, string symbol)
    {
        Code = code;
        Symbol = symbol;
    }

    public static Result<Currency> Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result<Currency>.Failure("Currency code cannot be null or empty");

        var normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode.Length != 3)
            return Result<Currency>.Failure("Currency code must be exactly 3 characters");

        if (!IsValidCurrencyCode(normalizedCode))
            return Result<Currency>.Failure("Currency code must contain only letters");

        var predefinedCurrency = GetPredefinedCurrency(normalizedCode);
        if (predefinedCurrency != null)
            return Result<Currency>.Success(predefinedCurrency);

        return Result<Currency>.Success(new Currency(normalizedCode, normalizedCode));
    }

    private static bool IsValidCurrencyCode(string code)
    {
        return code.All(char.IsLetter);
    }

    private static Currency? GetPredefinedCurrency(string code)
    {
        return code switch
        {
            "EUR" => EUR,
            "USD" => USD,
            "GBP" => GBP,
            "JPY" => JPY,
            "CHF" => CHF,
            _ => null
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }

    public override string ToString() => Code;
}
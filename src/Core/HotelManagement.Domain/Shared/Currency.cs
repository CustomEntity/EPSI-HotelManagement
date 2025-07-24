using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Shared;

public class Currency : ValueObject
{
    public static readonly Currency EUR = new("EUR", "€");
    public static readonly Currency USD = new("USD", "$");

    public string Code { get; }
    public string Symbol { get; }

    private Currency(string code, string symbol)
    {
        Code = code;
        Symbol = symbol;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}
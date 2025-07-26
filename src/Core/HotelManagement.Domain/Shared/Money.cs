using HotelManagement.Domain.Common;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Shared;

public class Money : ValueObject
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }

    public static Result<Money> Create(decimal amount, string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return Result<Money>.Failure("Currency code cannot be null or empty");

        if (amount < 0)
            return Result<Money>.Failure("Amount cannot be negative");

        var currency = currencyCode.ToUpperInvariant() switch
        {
            "EUR" => Currency.EUR,
            "USD" => Currency.USD,
            _ => null
        };

        if (currency == null)
            return Result<Money>.Failure($"Unsupported currency: {currencyCode}");

        try
        {
            var money = new Money(amount, currency);
            return Result<Money>.Success(money);
        }
        catch (ArgumentException ex)
        {
            return Result<Money>.Failure(ex.Message);
        }
    }

    public static Result<Money> Create(decimal amount, Currency currency)
    {
        if (amount < 0)
            return Result<Money>.Failure("Amount cannot be negative");

        if (currency == null)
            return Result<Money>.Failure("Currency cannot be null");

        try
        {
            var money = new Money(amount, currency);
            return Result<Money>.Success(money);
        }
        catch (ArgumentException ex)
        {
            return Result<Money>.Failure(ex.Message);
        }
    }

    public static Money Zero(Currency currency) => new(0, currency);
    public static Money ZeroEur() => new(0, Currency.EUR);
    public static Money ZeroUsd() => new(0, Currency.USD);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");

        var newAmount = Amount - other.Amount;
        if (newAmount < 0)
            throw new InvalidOperationException("Cannot create negative money amount");

        return new Money(newAmount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Factor cannot be negative", nameof(factor));

        return new Money(Amount * factor, Currency);
    }

    public Money Multiply(int factor)
    {
        if (factor < 0)
            throw new ArgumentException("Factor cannot be negative", nameof(factor));

        return new Money(Amount * factor, Currency);
    }

    public Money Divide(decimal divisor)
    {
        if (divisor <= 0)
            throw new ArgumentException("Divisor must be positive", nameof(divisor));

        return new Money(Amount / divisor, Currency);
    }

    public Money CalculatePercentage(decimal percentage)
    {
        if (percentage < 0)
            throw new ArgumentException("Percentage cannot be negative", nameof(percentage));

        return new Money(Amount * (percentage / 100), Currency);
    }

    public Money ApplyDiscount(decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException("Discount percentage must be between 0 and 100", nameof(discountPercentage));

        var discountAmount = CalculatePercentage(discountPercentage);
        return Subtract(discountAmount);
    }

    public bool IsZero => Amount == 0;
    public bool IsPositive => Amount > 0;

    public int CompareTo(Money other)
    {
        if (other == null)
            return 1;

        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies");

        return Amount.CompareTo(other.Amount);
    }

    public static bool operator >(Money left, Money right)
    {
        if (left == null || right == null)
            return false;
        return left.CompareTo(right) > 0;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left == null || right == null)
            return false;
        return left.CompareTo(right) < 0;
    }

    public static bool operator >=(Money left, Money right)
    {
        if (left == null || right == null)
            return false;
        return left.CompareTo(right) >= 0;
    }

    public static bool operator <=(Money left, Money right)
    {
        if (left == null || right == null)
            return false;
        return left.CompareTo(right) <= 0;
    }

    public static Money operator +(Money left, Money right)
    {
        if (left == null) throw new ArgumentNullException(nameof(left));
        if (right == null) throw new ArgumentNullException(nameof(right));
        
        return left.Add(right);
    }

    public static Money operator -(Money left, Money right)
    {
        if (left == null) throw new ArgumentNullException(nameof(left));
        if (right == null) throw new ArgumentNullException(nameof(right));
        
        return left.Subtract(right);
    }

    public static Money operator *(Money money, decimal factor)
    {
        if (money == null) throw new ArgumentNullException(nameof(money));
        
        return money.Multiply(factor);
    }

    public static Money operator *(Money money, int factor)
    {
        if (money == null) throw new ArgumentNullException(nameof(money));
        
        return money.Multiply(factor);
    }

    public static Money operator /(Money money, decimal divisor)
    {
        if (money == null) throw new ArgumentNullException(nameof(money));
        
        return money.Divide(divisor);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Currency.Symbol}{Amount:F2}";

    public string ToString(int decimals)
    {
        var format = $"F{decimals}";
        return $"{Currency.Symbol}{Amount.ToString(format)}";
    }

    public string ToStringWithoutSymbol() => $"{Amount:F2}";
}
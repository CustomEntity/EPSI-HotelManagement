using HotelManagement.Domain.Common;
using HotelManagement.Domain.Shared;
using System.Text.RegularExpressions;

namespace HotelManagement.Domain.Payment.ValueObjects;

public sealed class CreditCard : ValueObject
{
    public string CardNumber { get; }
    public string CardHolderName { get; }
    public int ExpiryMonth { get; }
    public int ExpiryYear { get; }
    public string Last4Digits { get; }
    public string CardType { get; }

    private CreditCard(
        string cardNumber, 
        string cardHolderName, 
        int expiryMonth, 
        int expiryYear)
    {
        CardNumber = MaskCardNumber(cardNumber);
        CardHolderName = cardHolderName;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        Last4Digits = cardNumber.Substring(cardNumber.Length - 4);
        CardType = DetectCardType(cardNumber);
    }

    public static Result<CreditCard> Create(
        string cardNumber,
        string cardHolderName,
        int expiryMonth,
        int expiryYear,
        string cvv)
    {
        // Nettoyer le numéro de carte
        cardNumber = Regex.Replace(cardNumber, @"[\s-]", "");

        // Validation du numéro de carte
        if (!IsValidCardNumber(cardNumber))
            return Result<CreditCard>.Failure("Invalid credit card number");

        // Validation du nom
        if (string.IsNullOrWhiteSpace(cardHolderName))
            return Result<CreditCard>.Failure("Card holder name is required");

        if (cardHolderName.Length > 100)
            return Result<CreditCard>.Failure("Card holder name is too long");

        // Validation de la date d'expiration
        if (expiryMonth < 1 || expiryMonth > 12)
            return Result<CreditCard>.Failure("Invalid expiry month");

        var currentYear = DateTime.UtcNow.Year;
        var currentMonth = DateTime.UtcNow.Month;

        if (expiryYear < currentYear || 
            (expiryYear == currentYear && expiryMonth < currentMonth))
            return Result<CreditCard>.Failure("Card has expired");

        if (expiryYear > currentYear + 20)
            return Result<CreditCard>.Failure("Invalid expiry year");

        // Validation du CVV
        if (!IsValidCvv(cvv, cardNumber))
            return Result<CreditCard>.Failure("Invalid CVV");

        return Result<CreditCard>.Success(new CreditCard(
            cardNumber, 
            cardHolderName.Trim(), 
            expiryMonth, 
            expiryYear));
    }

    private static bool IsValidCardNumber(string cardNumber)
    {
        // Algorithme de Luhn
        if (string.IsNullOrWhiteSpace(cardNumber) || 
            !Regex.IsMatch(cardNumber, @"^\d{13,19}$"))
            return false;

        int sum = 0;
        bool isEven = false;

        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            int digit = cardNumber[i] - '0';

            if (isEven)
            {
                digit *= 2;
                if (digit > 9)
                    digit -= 9;
            }

            sum += digit;
            isEven = !isEven;
        }

        return sum % 10 == 0;
    }

    private static bool IsValidCvv(string cvv, string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cvv))
            return false;

        // American Express a un CVV à 4 chiffres
        var isAmex = cardNumber.StartsWith("34") || cardNumber.StartsWith("37");
        var expectedLength = isAmex ? 4 : 3;

        return Regex.IsMatch(cvv, $@"^\d{{{expectedLength}}}$");
    }

    private static string DetectCardType(string cardNumber)
    {
        if (cardNumber.StartsWith("4"))
            return "Visa";
        if (cardNumber.StartsWith("5") && int.Parse(cardNumber.Substring(0, 2)) >= 51 && 
            int.Parse(cardNumber.Substring(0, 2)) <= 55)
            return "MasterCard";
        if (cardNumber.StartsWith("34") || cardNumber.StartsWith("37"))
            return "American Express";
        if (cardNumber.StartsWith("6011") || cardNumber.StartsWith("65"))
            return "Discover";
        
        return "Unknown";
    }

    private static string MaskCardNumber(string cardNumber)
    {
        if (cardNumber.Length <= 4)
            return cardNumber;

        var masked = new string('*', cardNumber.Length - 4);
        return masked + cardNumber.Substring(cardNumber.Length - 4);
    }

    public bool IsExpired()
    {
        var currentDate = DateTime.UtcNow;
        var expiryDate = new DateTime(ExpiryYear, ExpiryMonth, 1).AddMonths(1).AddDays(-1);
        return currentDate > expiryDate;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Last4Digits;
        yield return CardHolderName;
        yield return ExpiryMonth;
        yield return ExpiryYear;
    }

    public override string ToString() => $"{CardType} ending in {Last4Digits}";
}

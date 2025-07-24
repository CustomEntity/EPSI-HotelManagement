using HotelManagement.Domain.Common;
using System.Text.RegularExpressions;

namespace HotelManagement.Domain.Customer.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$", // E.164 format
        RegexOptions.Compiled);

    public string Value { get; }
    public string CountryCode { get; }
    public string Number { get; }

    private PhoneNumber(string value, string countryCode, string number)
    {
        Value = value;
        CountryCode = countryCode;
        Number = number;
    }

    public static PhoneNumber Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be null or empty", nameof(phoneNumber));

        // Nettoyer le numéro (supprimer espaces, tirets, parenthèses)
        var cleaned = Regex.Replace(phoneNumber.Trim(), @"[\s\-\(\)]", "");

        if (!PhoneRegex.IsMatch(cleaned))
            throw new ArgumentException("Invalid phone number format", nameof(phoneNumber));

        // Extraire l'indicatif pays (simplification)
        string countryCode = "";
        string number = cleaned;
        
        if (cleaned.StartsWith("+"))
        {
            var digitsOnly = cleaned.Substring(1);
            if (digitsOnly.Length >= 10)
            {
                countryCode = "+" + digitsOnly.Substring(0, digitsOnly.Length - 9);
                number = digitsOnly.Substring(digitsOnly.Length - 9);
            }
        }
        else if (cleaned.StartsWith("0") && cleaned.Length == 10)
        {
            // Numéro français
            countryCode = "+33";
            number = cleaned.Substring(1);
        }

        return new PhoneNumber(cleaned, countryCode, number);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
    public static explicit operator PhoneNumber(string value) => Create(value);
}
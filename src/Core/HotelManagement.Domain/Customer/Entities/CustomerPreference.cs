using HotelManagement.Domain.Common;
using HotelManagement.Domain.Customer.ValueObjects;

namespace HotelManagement.Domain.Customer.Entities;

public class CustomerPreference : Entity
{
    public CustomerId CustomerId { get; private set; }
    public string PreferenceType { get; private set; }
    public string PreferenceValue { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private CustomerPreference() { }

    private CustomerPreference(
        CustomerId customerId, 
        string preferenceType, 
        string preferenceValue, 
        string? description = null)
    {
        CustomerId = customerId;
        PreferenceType = preferenceType;
        PreferenceValue = preferenceValue;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public static CustomerPreference Create(
        CustomerId customerId, 
        string preferenceType, 
        string preferenceValue, 
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(preferenceType))
            throw new ArgumentException("Preference type cannot be null or empty", nameof(preferenceType));

        if (string.IsNullOrWhiteSpace(preferenceValue))
            throw new ArgumentException("Preference value cannot be null or empty", nameof(preferenceValue));

        return new CustomerPreference(customerId, preferenceType.Trim(), preferenceValue.Trim(), description?.Trim());
    }

    public void UpdateValue(string newValue, string? newDescription = null)
    {
        if (string.IsNullOrWhiteSpace(newValue))
            throw new ArgumentException("Preference value cannot be null or empty", nameof(newValue));

        PreferenceValue = newValue.Trim();
        Description = newDescription?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
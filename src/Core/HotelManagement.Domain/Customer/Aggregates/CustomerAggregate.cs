using HotelManagement.Domain.Common;
using HotelManagement.Domain.Customer.ValueObjects;
using HotelManagement.Domain.Customer.Events;
using HotelManagement.Domain.Customer.Entities;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Customer.Aggregates;

public class CustomerAggregate : AggregateRoot<CustomerId>
{
    public CustomerId Id { get; private set; }
    public PersonalName Name { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public Address? Address { get; private set; }
    public CustomerType CustomerType { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<CustomerPreference> _preferences = new();
    public IReadOnlyCollection<CustomerPreference> Preferences => _preferences.AsReadOnly();

    private CustomerAggregate() { } // EF Core

    private CustomerAggregate(
        CustomerId id,
        PersonalName name,
        Email email,
        CustomerType customerType,
        PhoneNumber? phoneNumber = null,
        Address? address = null)
    {
        Id = id;
        Name = name;
        Email = email;
        CustomerType = customerType;
        PhoneNumber = phoneNumber;
        Address = address;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;

        AddDomainEvent(new CustomerRegisteredEvent(Id, Email, Name, CustomerType));
    }

    public static Result<CustomerAggregate> Create(
        PersonalName name,
        Email email,
        CustomerType customerType,
        PhoneNumber? phoneNumber = null,
        Address? address = null)
    {
        try
        {
            var customerId = CustomerId.New();
            var customer = new CustomerAggregate(customerId, name, email, customerType, phoneNumber, address);
            return Result<CustomerAggregate>.Success(customer);
        }
        catch (Exception ex)
        {
            return Result<CustomerAggregate>.Failure($"Failed to create customer: {ex.Message}");
        }
    }

    public Result UpdatePersonalInfo(PersonalName newName, Email newEmail)
    {
        try
        {
            var previousName = Name;
            var previousEmail = Email;

            Name = newName;
            Email = newEmail;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new CustomerUpdatedEvent(Id, previousEmail, newEmail, previousName, newName));
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to update customer: {ex.Message}");
        }
    }

    public Result UpdateContactInfo(PhoneNumber? phoneNumber, Address? address)
    {
        try
        {
            PhoneNumber = phoneNumber;
            Address = address;
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to update contact info: {ex.Message}");
        }
    }

    public Result UpgradeToVIP()
    {
        if (CustomerType.IsVIP)
            return Result.Failure("Customer is already VIP");

        CustomerType = ValueObjects.CustomerType.VIP;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result AddPreference(string preferenceType, string preferenceValue, string? description = null)
    {
        try
        {
            // Vérifier si la préférence existe déjà
            var existingPreference = _preferences.FirstOrDefault(p => p.PreferenceType == preferenceType);
            if (existingPreference != null)
            {
                existingPreference.UpdateValue(preferenceValue, description);
            }
            else
            {
                var preference = CustomerPreference.Create(Id, preferenceType, preferenceValue, description);
                _preferences.Add(preference);
            }

            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to add preference: {ex.Message}");
        }
    }

    public Result RemovePreference(string preferenceType)
    {
        var preference = _preferences.FirstOrDefault(p => p.PreferenceType == preferenceType);
        if (preference == null)
            return Result.Failure("Preference not found");

        _preferences.Remove(preference);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure("Customer is already inactive");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CustomerDeletedEvent(Id, Email, Name));
        return Result.Success();
    }

    public Result Reactivate()
    {
        if (IsActive)
            return Result.Failure("Customer is already active");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public bool HasPreference(string preferenceType) =>
        _preferences.Any(p => p.PreferenceType == preferenceType);

    public string? GetPreferenceValue(string preferenceType) =>
        _preferences.FirstOrDefault(p => p.PreferenceType == preferenceType)?.PreferenceValue;

    public bool CanMakeBooking() => IsActive;

    public decimal GetDiscountRate()
    {
        return CustomerType.Value switch
        {
            "VIP" => 0.15m,      // 15% de réduction pour les VIP
            "Corporate" => 0.10m, // 10% de réduction pour les entreprises
            _ => 0m               // Pas de réduction pour les particuliers
        };
    }
}
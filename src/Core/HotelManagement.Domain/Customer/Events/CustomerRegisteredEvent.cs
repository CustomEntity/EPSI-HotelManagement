using HotelManagement.Domain.Common;
using HotelManagement.Domain.Customer.ValueObjects;

namespace HotelManagement.Domain.Customer.Events;

public sealed class CustomerRegisteredEvent : DomainEventBase
{
    public CustomerId CustomerId { get; }
    public Email Email { get; }
    public PersonalName Name { get; }
    public CustomerType CustomerType { get; }

    public CustomerRegisteredEvent(CustomerId customerId, Email email, PersonalName name, CustomerType customerType)
    {
        CustomerId = customerId;
        Email = email;
        Name = name;
        CustomerType = customerType;
    }
}

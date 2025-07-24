using HotelManagement.Domain.Common;
using HotelManagement.Domain.Customer.ValueObjects;

namespace HotelManagement.Domain.Customer.Events;

public sealed class CustomerDeletedEvent : DomainEventBase
{
    public CustomerId CustomerId { get; }
    public Email Email { get; }
    public PersonalName Name { get; }
    public DateTime DeletionDate { get; }

    public CustomerDeletedEvent(CustomerId customerId, Email email, PersonalName name)
    {
        CustomerId = customerId;
        Email = email;
        Name = name;
        DeletionDate = DateTime.UtcNow;
    }
}
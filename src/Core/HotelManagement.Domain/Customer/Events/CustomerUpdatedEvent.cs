using HotelManagement.Domain.Common;
using HotelManagement.Domain.Customer.ValueObjects;

namespace HotelManagement.Domain.Customer.Events;

public sealed class CustomerUpdatedEvent : DomainEventBase
{
    public CustomerId CustomerId { get; }
    public Email? PreviousEmail { get; }
    public Email? NewEmail { get; }
    public PersonalName? PreviousName { get; }
    public PersonalName? NewName { get; }

    public CustomerUpdatedEvent(
        CustomerId customerId, 
        Email? previousEmail = null, 
        Email? newEmail = null,
        PersonalName? previousName = null,
        PersonalName? newName = null)
    {
        CustomerId = customerId;
        PreviousEmail = previousEmail;
        NewEmail = newEmail;
        PreviousName = previousName;
        NewName = newName;
    }
}

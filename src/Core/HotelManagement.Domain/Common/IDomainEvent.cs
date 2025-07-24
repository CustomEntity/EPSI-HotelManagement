using MediatR;

namespace HotelManagement.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
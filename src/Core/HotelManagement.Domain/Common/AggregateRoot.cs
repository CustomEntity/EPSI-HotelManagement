namespace HotelManagement.Domain.Common;

public abstract class AggregateRoot<TId> : Entity where TId : notnull
{
    public TId Id { get; protected set; } = default!;
    public DateTime CreatedAt { get; protected set; }
    public DateTime? ModifiedAt { get; protected set; }
}
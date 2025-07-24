using HotelManagement.Domain.Common;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Room.Entities;

public class RoomType : Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Money BasePrice { get; private set; }
    public int MaxOccupancy { get; private set; }

    private RoomType() 
    {
        Name = string.Empty;
        BasePrice = new Money(0, Currency.EUR);
    }

    public RoomType(string name, Money basePrice, int maxOccupancy)
    {
        Id = Guid.NewGuid();
        Name = name;
        BasePrice = basePrice;
        MaxOccupancy = maxOccupancy;
    }

    public void UpdatePrice(Money newPrice)
    {
        BasePrice = newPrice;
    }
}
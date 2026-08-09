using Seatly.Domain.Entities;
using Seatly.Domain.ValueObjects;

namespace Seatly.Application.Interfaces;

public interface IDiscountStrategy
{
    string Name { get; }
    int Priority { get; }

    
    bool IsApplicable(Event @event, User user, int numberOfSeats, string categoryName);

  
    Money ApplyDiscount(Money originalPrice);
}

using Seatly.Domain.ValueObjects;

namespace Seatly.Application.Interfaces;

public interface IDiscountStrategy
{
    string Name { get; }
    int Priority { get; } // 1 = prvi se primjenjuje
    Money ApplyDiscount(Money originalPrice);
}

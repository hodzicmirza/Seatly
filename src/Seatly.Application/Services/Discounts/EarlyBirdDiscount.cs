using Seatly.Application.Interfaces;
using Seatly.Domain.ValueObjects;

namespace Seatly.Application.Services.Discounts;

public class EarlyBirdDiscount : IDiscountStrategy
{
    public string Name => "Early Bird (10% popusta)";
    public int Priority => 1;

    public Money ApplyDiscount(Money originalPrice)
    {
        return originalPrice * 0.9m; // 10% popusta = množimo sa 0.9
    }
}

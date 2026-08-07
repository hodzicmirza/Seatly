using Seatly.Application.Interfaces;
using Seatly.Domain.ValueObjects;

namespace Seatly.Application.Services.Discounts;

public class VIPDiscount : IDiscountStrategy
{
    public string Name => "VIP (20% popusta)";
    public int Priority => 2;

    public Money ApplyDiscount(Money originalPrice)
    {
        return originalPrice * 0.8m; // 20% popusta = množimo sa 0.8
    }
}

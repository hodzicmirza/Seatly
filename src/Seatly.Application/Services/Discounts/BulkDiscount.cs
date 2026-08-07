using Seatly.Application.Interfaces;
using Seatly.Domain.ValueObjects;

namespace Seatly.Application.Services.Discounts;

public class BulkDiscount : IDiscountStrategy
{
    public string Name => "Grupna rezervacija (15% popusta)";
    public int Priority => 3;

    public Money ApplyDiscount(Money originalPrice)
    {
        return originalPrice * 0.85m; // 15% popusta za 5+ karata
    }
}

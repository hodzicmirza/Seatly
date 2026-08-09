using Seatly.Application.Interfaces;
using Seatly.Domain.Entities;
using Seatly.Domain.ValueObjects;

namespace Seatly.Application.Services.Discounts;

// Strategy Pattern Implementation: Bulk Reservation Discount
// Applies 15% discount automatically whenever a customer reserves 5 or more seats in a single transaction.
public class BulkDiscount : IDiscountStrategy
{
    public string Name => "Bulk Purchase (15% Discount)";
    public int Priority => 3; // Priority 3 for group purchases

    public bool IsApplicable(Event @event, User user, int numberOfSeats, string categoryName)
    {
        return numberOfSeats >= 5;
    }

    public Money ApplyDiscount(Money originalPrice)
    {
        return originalPrice * 0.85m; // Deducts 15% for group bookings (5+ seats)
    }
}

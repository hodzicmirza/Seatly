using Seatly.Application.Interfaces;
using Seatly.Domain.Entities;
using Seatly.Domain.ValueObjects;

namespace Seatly.Application.Services.Discounts;

// Strategy Pattern Implementation: Early Bird Discount Strategy
// Grants a 10% discount if the event date is more than 7 days in the future from booking time.
public class EarlyBirdDiscount : IDiscountStrategy
{
    public string Name => "Early Bird (10% Discount)";
    public int Priority => 1;

    public bool IsApplicable(Event @event, User user, int numberOfSeats, string categoryName)
    {
        // Applies to events scheduled at least 7 days ahead
        return @event.Date > DateTime.UtcNow.AddDays(7);
    }

    public Money ApplyDiscount(Money originalPrice)
    {
        return originalPrice * 0.9m; // Subtracts 10% from base category price
    }
}

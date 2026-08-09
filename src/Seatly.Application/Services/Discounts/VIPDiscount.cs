using Seatly.Application.Interfaces;
using Seatly.Domain.Entities;
using Seatly.Domain.Enums;
using Seatly.Domain.ValueObjects;

namespace Seatly.Application.Services.Discounts;

// Strategy Pattern Implementation: VIP Discount Strategy
// Highest priority strategy (Priority = 4) that grants 20% discount if ticket category is VIP or user is an Admin/Organizer.
public class VIPDiscount : IDiscountStrategy
{
    public string Name => "VIP Discount (20% Discount)";
    public int Priority => 4; // Highest priority for VIP ticket categories or administrative roles

    public bool IsApplicable(Event @event, User user, int numberOfSeats, string categoryName)
    {
        var isVipCategory = categoryName.Contains("VIP", StringComparison.OrdinalIgnoreCase);
        var isVipUser = user.Role == UserRole.Admin || user.Role == UserRole.Organizer;

        return isVipCategory || isVipUser;
    }

    public Money ApplyDiscount(Money originalPrice)
    {
        return originalPrice * 0.8m; // Deducts 20% from price
    }
}

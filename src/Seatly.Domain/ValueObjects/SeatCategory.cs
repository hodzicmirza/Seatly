namespace Seatly.Domain.ValueObjects;

public record SeatCategory(string Name, decimal PriceMultiplier, int SeatsCount = 0);

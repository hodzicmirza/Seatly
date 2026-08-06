using Seatly.Domain.Exceptions;
using Seatly.Domain.ValueObjects;

namespace Seatly.Domain.Entities;

public abstract class Event
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DateTime Date { get; private set; }
    public Address Location { get; private set; } = null!;
    public Money BasePrice { get; private set; } = null!;
    public int TotalSeats { get; private set; }
    public List<SeatCategory> SeatCategories { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }

    protected Event() { } // contructor for database

    protected Event(
        string name,
        string description,
        DateTime date,
        Address location,
        Money basePrice,
        int totalSeats,
        List<SeatCategory> categories
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Event name is required.");
        }

        if (totalSeats <= 0)
        {
            throw new DomainException("Total seats number must be greater that 0.");
        }

        if (date < DateTime.UtcNow)
        {
            throw new DomainException("Date of the event cannot be in the past.");
        }

        if (categories == null || !categories.Any())
        {
            throw new DomainException("At least one seat category is required.");
        }

        this.Id = Guid.NewGuid();
        this.Name = name;
        this.Description = description;
        this.Date = date;
        this.Location = location;
        this.BasePrice = basePrice;
        this.TotalSeats = totalSeats;
        this.SeatCategories = categories;
        this.CreatedAt = DateTime.UtcNow;
    }

    public abstract bool IsRefundable();

    public virtual Money CalculatePrice(SeatCategory category)
    {
        return BasePrice * category.PriceMultiplier;
    }

    public int AvailableSeats(int bookedSeats) => this.TotalSeats - bookedSeats;

    public bool HasAvailableSeats(int requestedSeats, int bookedSeats) =>
        AvailableSeats(bookedSeats) >= requestedSeats;
}

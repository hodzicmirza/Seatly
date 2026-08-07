using Seatly.Domain.Enums;
using Seatly.Domain.ValueObjects;

namespace Seatly.Domain.Entities;

public class Concert : Event
{
    public string Headliner { get; private set; } = null!;
    public string? SupportAct { get; private set; }

    private Concert() { }

    public Concert(
        string name,
        string description,
        DateTime date,
        Address location,
        Money basePrice,
        int totalSeats,
        List<SeatCategory> categories,
        string headliner,
        string? supportAct = null
    )
        : base(
            name,
            description,
            date,
            location,
            basePrice,
            totalSeats,
            categories,
            EventType.Concert
        )
    {
        if (string.IsNullOrWhiteSpace(headliner))
        {
            throw new ArgumentException("Headliner is required for concert");
        }

        this.Headliner = headliner;
        this.SupportAct = supportAct;
    }

    public override bool IsRefundable()
    {
        return this.Date > DateTime.UtcNow.AddDays(7);
    }
}

using Seatly.Domain.ValueObjects;

namespace Seatly.Domain.Entities;

public class Conference : Event
{
    public string Organizer { get; private set; } = null!;
    public string? KeynoteSpeaker { get; private set; }

    private Conference() { }

    public Conference(
        string name,
        string description,
        DateTime date,
        Address location,
        Money basePrice,
        int totalSeats,
        List<SeatCategory> categories,
        string organizer,
        string? keynoteSpeaker = null
    )
        : base(name, description, date, location, basePrice, totalSeats, categories)
    {
        if (string.IsNullOrWhiteSpace(organizer))
        {
            throw new ArgumentException("Organizer is required for a conference");
        }

        this.Organizer = organizer;
        this.KeynoteSpeaker = keynoteSpeaker;
    }

    public override bool IsRefundable() => true;
}

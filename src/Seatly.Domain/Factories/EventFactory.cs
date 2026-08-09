using Seatly.Domain.Entities;
using Seatly.Domain.Enums;
using Seatly.Domain.Exceptions;
using Seatly.Domain.ValueObjects;

namespace Seatly.Domain.Factories;

// Factory Pattern implementation: Instantiates specific Event subclasses (Concert vs Conference)
// based on the requested EventType enum without exposing instantiation details to the application layer.
public static class EventFactory
{
    public static Event CreateEvent(
        string name,
        string description,
        DateTime date,
        Address location,
        Money basePrice,
        int totalSeats,
        List<SeatCategory> categories,
        EventType eventType,
        string? extraInfo = null,
        string? subInfo = null
    )
    {
        // Switch expression maps enum to concrete domain entity constructors
        return eventType switch
        {
            EventType.Concert => new Concert(
                name,
                description,
                date,
                location,
                basePrice,
                totalSeats,
                categories,
                headliner: extraInfo ?? "Guest Performer",
                supportAct: subInfo
            ),
            EventType.Conference => new Conference(
                name,
                description,
                date,
                location,
                basePrice,
                totalSeats,
                categories,
                organizer: extraInfo ?? "Organizer Team",
                keynoteSpeaker: subInfo
            ),
            _ => throw new DomainException($"Unsupported event type: {eventType}")
        };
    }
}

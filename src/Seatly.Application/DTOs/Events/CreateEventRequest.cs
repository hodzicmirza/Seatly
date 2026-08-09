using Seatly.Domain.Enums;

namespace Seatly.Application.DTOs.Events;

public record CreateEventRequest(
    string Name,
    string Description,
    DateTime Date,
    string Street,
    string City,
    string Country,
    decimal BasePrice,
    int TotalSeats,
    List<SeatCategoryDto> Categories,
    EventType EventType,
    string? Headliner, // samo za Concert
    string? SupportAct, // samo za Concert
    string? Organizer, // samo za Conference
    string? KeynoteSpeaker // samo za Conference
);

public record SeatCategoryDto(string Name, decimal Multiplier, int SeatsCount = 0);

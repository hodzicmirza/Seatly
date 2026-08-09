using Seatly.Domain.Entities;
using Seatly.Domain.Enums;

namespace Seatly.Application.DTOs.Events;

public record EventResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime Date,
    string Street,
    string City,
    string Country,
    decimal BasePrice,
    int TotalSeats,
    int AvailableSeats,
    string EventType,
    List<CategoryResponse> Categories,
    string? Headliner = null,
    string? SupportAct = null,
    string? Organizer = null,
    string? KeynoteSpeaker = null
);

public record CategoryResponse(string Name, decimal PriceMultiplier, decimal FinalPrice, int SeatsCount = 0);

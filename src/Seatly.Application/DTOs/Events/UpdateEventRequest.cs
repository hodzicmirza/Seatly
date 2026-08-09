namespace Seatly.Application.DTOs.Events;

public record UpdateEventRequest(
    string Name,
    string Description,
    DateTime Date,
    string Street,
    string City,
    string Country,
    decimal BasePrice,
    int TotalSeats,
    List<SeatCategoryDto>? Categories = null
);

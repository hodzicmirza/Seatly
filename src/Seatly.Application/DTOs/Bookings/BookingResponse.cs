namespace Seatly.Application.DTOs.Bookings;

public record BookingResponse(
    Guid BookingId,
    string EventName,
    DateTime EventDate,
    string EventLocation,
    string Category,
    int NumberOfSeats,
    decimal TotalPrice,
    string Status,
    string? QrCodeBase64,
    DateTime CreatedAt
);

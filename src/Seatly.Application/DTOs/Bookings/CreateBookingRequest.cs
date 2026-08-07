namespace Seatly.Application.DTOs.Bookings;

public record CreateBookingRequest(Guid EventId, int NumberOfSeats, string CategoryName);

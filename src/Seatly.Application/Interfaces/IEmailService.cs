using Seatly.Application.DTOs.Bookings;

namespace Seatly.Application.Interfaces;

public interface IEmailService
{
    Task SendBookingConfirmationAsync(string toEmail, BookingResponse booking, string qrCodeBase64);
    Task SendBookingCancellationAsync(string toEmail, string eventName, DateTime eventDate);
}

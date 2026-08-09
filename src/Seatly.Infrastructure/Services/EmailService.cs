using Microsoft.Extensions.Logging;
using Seatly.Application.DTOs.Bookings;
using Seatly.Application.Interfaces;

namespace Seatly.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly string _apiKey;
    private readonly string _senderEmail;
    private readonly bool _isConfigured;
    private readonly ILogger<EmailService>? _logger;

    public EmailService(string apiKey, string senderEmail, ILogger<EmailService>? logger = null)
    {
        _apiKey = apiKey;
        _senderEmail = senderEmail;
        _isConfigured = !string.IsNullOrEmpty(apiKey);
        _logger = logger;
    }

    public async Task SendBookingConfirmationAsync(
        string toEmail,
        BookingResponse booking,
        string qrCodeBase64
    )
    {
        if (!_isConfigured)
        {
            _logger?.LogInformation(
                "Email skipped (not configured) - Booking confirmation for {ToEmail}, Event: {EventName}, Total: {Total}",
                toEmail,
                booking.EventName,
                booking.TotalPrice
            );
            return;
        }

        _logger?.LogInformation("Sending booking confirmation email to {ToEmail}", toEmail);
        await Task.Delay(100);
    }

    public async Task SendBookingCancellationAsync(
        string toEmail,
        string eventName,
        DateTime eventDate
    )
    {
        if (!_isConfigured)
        {
            _logger?.LogInformation("Email skipped (not configured) - Cancellation for {ToEmail}", toEmail);
            return;
        }

        _logger?.LogInformation("Sending cancellation email to {ToEmail}", toEmail);
        await Task.Delay(100);
    }
}

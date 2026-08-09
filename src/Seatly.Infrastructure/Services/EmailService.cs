using System.Net.Http.Headers;
using System.Net.Http.Json;
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
        _senderEmail = string.IsNullOrEmpty(senderEmail) ? "onboarding@resend.dev" : senderEmail;
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

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var htmlContent = $@"
                <div style='font-family: Arial, sans-serif; padding: 24px; color: #1f2937; max-width: 600px; margin: 0 auto; border: 1px solid #e5e7eb; border-radius: 12px; background-color: #ffffff;'>
                    <h2 style='color: #4f46e5; margin-top: 0;'>Thank you for your booking! 🎟️</h2>
                    <p>Your reservation for <strong>{booking.EventName}</strong> has been successfully confirmed.</p>
                    
                    <table style='width: 100%; border-collapse: collapse; margin: 20px 0; font-size: 14px;'>
                        <tr style='border-bottom: 1px solid #f3f4f6;'><td style='padding: 10px 0; color: #6b7280;'><strong>Event:</strong></td><td style='padding: 10px 0;'>{booking.EventName}</td></tr>
                        <tr style='border-bottom: 1px solid #f3f4f6;'><td style='padding: 10px 0; color: #6b7280;'><strong>Date & Time:</strong></td><td style='padding: 10px 0;'>{booking.EventDate:MMMM dd, yyyy - HH:mm}</td></tr>
                        <tr style='border-bottom: 1px solid #f3f4f6;'><td style='padding: 10px 0; color: #6b7280;'><strong>Location:</strong></td><td style='padding: 10px 0;'>{booking.EventLocation}</td></tr>
                        <tr style='border-bottom: 1px solid #f3f4f6;'><td style='padding: 10px 0; color: #6b7280;'><strong>Category:</strong></td><td style='padding: 10px 0;'>{booking.Category}</td></tr>
                        <tr style='border-bottom: 1px solid #f3f4f6;'><td style='padding: 10px 0; color: #6b7280;'><strong>Tickets:</strong></td><td style='padding: 10px 0;'>{booking.NumberOfSeats}</td></tr>
                        <tr style='border-bottom: 1px solid #f3f4f6;'><td style='padding: 10px 0; color: #6b7280;'><strong>Total Amount:</strong></td><td style='padding: 10px 0; font-weight: bold; color: #111827;'>{booking.TotalPrice} BAM</td></tr>
                    </table>

                    <div style='text-align: center; margin: 24px 0; padding: 16px; background-color: #f9fafb; border-radius: 8px;'>
                        <p style='margin-bottom: 12px; font-weight: 500; color: #374151;'>Scan this QR code at the entrance:</p>
                        <img src='data:image/png;base64,{qrCodeBase64}' alt='Ticket QR Code' style='width: 200px; height: 200px; border-radius: 8px;' />
                    </div>

                    <p style='font-size: 12px; color: #9ca3af; text-align: center; margin-bottom: 0;'>Seatly Event Management System &copy; {DateTime.UtcNow.Year}</p>
                </div>
            ";

            var payload = new
            {
                from = _senderEmail,
                to = new[] { toEmail },
                subject = $"Seatly Ticket — {booking.EventName}",
                html = htmlContent
            };

            var response = await client.PostAsJsonAsync("https://api.resend.com/emails", payload);
            if (response.IsSuccessStatusCode)
            {
                _logger?.LogInformation("Successfully sent booking confirmation email to {ToEmail}", toEmail);
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger?.LogError("Failed to send email to {ToEmail}. Response: {Response}", toEmail, err);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Exception thrown while sending booking confirmation email to {ToEmail}", toEmail);
        }
    }

    public async Task SendBookingCancellationAsync(
        string toEmail,
        string eventName,
        DateTime eventDate
    )
    {
        if (!_isConfigured) return;

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var htmlContent = $@"
                <div style='font-family: Arial, sans-serif; padding: 24px; color: #1f2937; max-width: 600px; margin: 0 auto; border: 1px solid #e5e7eb; border-radius: 12px; background-color: #ffffff;'>
                    <h2 style='color: #ef4444; margin-top: 0;'>Booking Cancelled ⚠️</h2>
                    <p>Your pending reservation for <strong>{eventName}</strong> scheduled for <strong>{eventDate:MMMM dd, yyyy - HH:mm}</strong> has been cancelled due to expiration.</p>
                    <p style='font-size: 14px; color: #6b7280;'>If this was a mistake, you can visit Seatly to re-book your seats.</p>
                </div>
            ";

            var payload = new
            {
                from = _senderEmail,
                to = new[] { toEmail },
                subject = $"Seatly — Reservation Cancelled ({eventName})",
                html = htmlContent
            };

            await client.PostAsJsonAsync("https://api.resend.com/emails", payload);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Exception while sending cancellation email to {ToEmail}", toEmail);
        }
    }
}

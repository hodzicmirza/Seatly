using Seatly.Domain.Enums;
using Seatly.Domain.Interfaces;

namespace Seatly.API.BackgroundServices;

// Hosted Background Service: Periodically scans for Pending reservations (older than 15 mins)
// and transitions them to Cancelled to release held seats back into the available pool.
public class ExpiredBookingCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiredBookingCleanupService> _logger;

    public ExpiredBookingCleanupService(
        IServiceProvider serviceProvider,
        ILogger<ExpiredBookingCleanupService> logger
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Expired Booking Cleanup Background Service is running...");

        // Polling loop that executes continuously until the web application shuts down
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Create a scoped DI container to resolve Scoped repositories within Singleton BackgroundService lifecycle
                using var scope = _serviceProvider.CreateScope();
                var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var allBookings = await bookingRepo.GetAllAsync();
                // Filter for unconfirmed Pending bookings created more than 15 minutes ago
                var expiredBookings = allBookings
                    .Where(b =>
                        b.Status == BookingStatus.Pending
                        && b.CreatedAt < DateTime.UtcNow.AddMinutes(-15)
                    )
                    .ToList();

                if (expiredBookings.Any())
                {
                    _logger.LogInformation(
                        "Cancelling {Count} expired pending bookings...",
                        expiredBookings.Count
                    );
                    foreach (var booking in expiredBookings)
                    {
                        booking.Cancel();
                        await bookingRepo.UpdateAsync(booking);
                    }
                    await unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired bookings.");
            }

            // Wait 2 minutes before the next cleanup cycle
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}

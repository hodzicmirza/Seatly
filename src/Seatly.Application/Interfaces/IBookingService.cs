using Seatly.Application.Common;
using Seatly.Application.DTOs.Bookings;

namespace Seatly.Application.Interfaces;

public interface IBookingService
{
    Task<Result<BookingResponse>> CreateBookingAsync(
        CreateBookingRequest request,
        string supabaseUserId,
        string userEmail
    );

    Task<Result> CancelBookingAsync(Guid bookingId, string supabaseUserId);

    Task<Result<BookingResponse>> GetBookingByIdAsync(Guid bookingId);

    Task<Result<IEnumerable<BookingResponse>>> GetUserBookingsAsync(string supabaseUserId);

    Task<Result<IEnumerable<BookingResponse>>> GetAllBookingsAsync();

    Task<Result> DeleteBookingAdminAsync(Guid bookingId);

    Task<Result> MarkBookingAsUsedAsync(Guid bookingId, string qrCodeData);
}

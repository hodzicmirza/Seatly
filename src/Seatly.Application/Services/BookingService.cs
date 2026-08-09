using System.Text.Json;
using Seatly.Application.Common;
using Seatly.Application.DTOs.Bookings;
using Seatly.Application.Interfaces;
using Seatly.Application.Services.Discounts;
using Seatly.Domain.Entities;
using Seatly.Domain.Exceptions;
using Seatly.Domain.Interfaces;
using Seatly.Domain.ValueObjects;

namespace Seatly.Application.Services;

// Application Service: Orchestrates seat reservation workflow, discount application,
// capacity validation, Unit of Work persistence, and QR code generation for tickets.
public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<IDiscountStrategy> _discountStrategies;
    private readonly IQrCodeService _qrCodeService;
    private readonly IEmailService _emailService;

    public BookingService(
        IBookingRepository bookingRepository,
        IEventRepository eventRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IEnumerable<IDiscountStrategy> discountStrategies,
        IQrCodeService qrCodeService,
        IEmailService emailService
    )
    {
        _bookingRepository = bookingRepository;
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        // Injected strategies sorted by Priority ascending so highest priority strategy is applied last
        _discountStrategies = discountStrategies.OrderBy(d => d.Priority);
        _qrCodeService = qrCodeService;
        _emailService = emailService;
    }

    public async Task<Result<BookingResponse>> CreateBookingAsync(
        CreateBookingRequest request,
        string supabaseUserId,
        string userEmail
    )
    {
        try
        {
            var user = await this.GetOrCreateUserAsync(supabaseUserId, userEmail);
            var @event = await this._eventRepository.GetByIdAsync(request.EventId);

            if (@event == null)
            {
                return Result<BookingResponse>.Failure("Event not found.");
            }
            var category = @event.SeatCategories.FirstOrDefault(c =>
                c.Name.Equals(request.CategoryName, StringComparison.OrdinalIgnoreCase)
            );

            if (category == null)
            {
                return Result<BookingResponse>.Failure(
                    $"Category '{request.CategoryName}' not found."
                );
            }

            // Calculate initial category price (Base Price * Category Multiplier)
            Money price = @event.CalculatePrice(category);

            // Strategy Pattern: Evaluate all registered IDiscountStrategy implementations to pick the highest priority applicable discount
            var applicableDiscount = this._discountStrategies
                .Where(d => d.IsApplicable(@event, user, request.NumberOfSeats, category.Name))
                .OrderByDescending(d => d.Priority)
                .FirstOrDefault();

            if (applicableDiscount != null)
            {
                price = applicableDiscount.ApplyDiscount(price);
            }

            // Real-time capacity validation against non-cancelled DB bookings
            var bookedSeates = await this._bookingRepository.GetBookedSeatsCountAsync(
                request.EventId
            );

            if (!@event.HasAvailableSeats(request.NumberOfSeats, bookedSeates))
            {
                return Result<BookingResponse>.Failure(
                    $"Not enough seats available. Available {@event.AvailableSeats(bookedSeates)}"
                );
            }

            var totalPrice = price * request.NumberOfSeats;
            var booking = new Booking(
                user.Id,
                @event.Id,
                request.NumberOfSeats,
                new SeatCategory(category.Name, category.PriceMultiplier),
                totalPrice
            );

            // Save initial Pending booking state via Unit of Work
            await this._bookingRepository.AddAsync(booking);
            await this._unitOfWork.SaveChangesAsync();

            // Generate Base64 QR code ticket payload
            var qrData = JsonSerializer.Serialize(
                new
                {
                    bookingId = booking.Id,
                    eventId = @event.Id,
                    eventName = @event.Name,
                    seats = request.NumberOfSeats,
                    category = category.Name,
                }
            );

            var qrCodeBase64 = await this._qrCodeService.GenerateQrCodeAsync(qrData);
            booking.Confirm(qrCodeBase64);
            await this._bookingRepository.UpdateAsync(booking);
            await this._unitOfWork.SaveChangesAsync();

            var response = MapToResponse(booking, @event);
            await this._emailService.SendBookingConfirmationAsync(user.Email, response, qrCodeBase64);

            return Result<BookingResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            return Result<BookingResponse>.Failure(ex.Message);
        }
    }

    public async Task<Result> CancelBookingAsync(Guid bookingId, string supabaseUserId)
    {
        var user = await this._userRepository.GetBySupabaseUserIdAsync(supabaseUserId);
        if (user == null)
        {
            return Result.Failure("User not found.");
        }

        var booking = await this._bookingRepository.GetByIdAsync(bookingId);
        if (booking == null)
        {
            return Result.Failure("Booking not found.");
        }

        if (booking.UserId != user.Id)
        {
            return Result.Failure("You don't have a permission to cancel this booking.");
        }

        try
        {
            // Transition booking state to Cancelled
            booking.Cancel();
            await this._bookingRepository.UpdateAsync(booking);
            await this._unitOfWork.SaveChangesAsync();

            await this._emailService.SendBookingCancellationAsync(
                user.Email,
                booking.Event.Name,
                booking.Event.Date
            );

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<BookingResponse>> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = await this._bookingRepository.GetByIdAsync(bookingId);
        if (booking == null)
            return Result<BookingResponse>.Failure("Booking not found.");

        return Result<BookingResponse>.Success(MapToResponse(booking, booking.Event));
    }

    public async Task<Result<IEnumerable<BookingResponse>>> GetUserBookingsAsync(
        string supabaseUserId
    )
    {
        var user = await this._userRepository.GetBySupabaseUserIdAsync(supabaseUserId);
        if (user == null)
        {
            return Result<IEnumerable<BookingResponse>>.Success(Enumerable.Empty<BookingResponse>());
        }

        var bookings = await this._bookingRepository.GetByUserIdAsync(user.Id);
        var responses = bookings.Select(b => MapToResponse(b, b.Event));

        return Result<IEnumerable<BookingResponse>>.Success(responses);
    }

    public async Task<Result<IEnumerable<BookingResponse>>> GetAllBookingsAsync()
    {
        var bookings = await this._bookingRepository.GetAllAsync();
        var responses = bookings.Select(b => MapToResponse(b, b.Event));
        return Result<IEnumerable<BookingResponse>>.Success(responses);
    }

    public async Task<Result> DeleteBookingAdminAsync(Guid bookingId)
    {
        var booking = await this._bookingRepository.GetByIdAsync(bookingId);
        if (booking == null)
            return Result.Failure("Booking not found.");

        await this._bookingRepository.DeleteAsync(booking);
        await this._unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> MarkBookingAsUsedAsync(Guid bookingId, string qrCodeData)
    {
        var booking = await this._bookingRepository.GetByIdAsync(bookingId);
        if (booking == null)
            return Result.Failure("Booking not found.");

        try
        {
            var isValid = await _qrCodeService.ValidateQrCodeAsync(qrCodeData);
            if (!isValid)
                return Result.Failure("Invalid QR code.");

            booking.MarkAsUsed();
            await this._bookingRepository.UpdateAsync(booking);
            await this._unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<BookingResponse>> ValidateQrTicketAsync(string qrCodeData)
    {
        if (string.IsNullOrWhiteSpace(qrCodeData))
            return Result<BookingResponse>.Failure("QR code or Ticket ID payload is required.");

        Guid bookingId = Guid.Empty;

        
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(qrCodeData);
            if (doc.RootElement.TryGetProperty("bookingId", out var idProp) && Guid.TryParse(idProp.GetString(), out var parsedId))
            {
                bookingId = parsedId;
            }
        }
        catch
        {
            Guid.TryParse(qrCodeData.Trim(), out bookingId);
        }

        Booking? booking = null;

        if (bookingId != Guid.Empty)
        {
            booking = await this._bookingRepository.GetByIdAsync(bookingId);
        }

        if (booking == null)
        {
            var allBookings = await this._bookingRepository.GetAllAsync();
            booking = allBookings.FirstOrDefault(b => b.QrCodeData == qrCodeData);
        }

        if (booking == null)
        {
            return Result<BookingResponse>.Failure("Invalid Ticket: Booking reference not found in database.");
        }

        if (booking.Status == Seatly.Domain.Enums.BookingStatus.Cancelled)
        {
            return Result<BookingResponse>.Failure($"Ticket CANCELLED! Booking #{booking.Id.ToString()[..8]} was cancelled.");
        }

        if (booking.Status == Seatly.Domain.Enums.BookingStatus.Used)
        {
            return Result<BookingResponse>.Failure($"Ticket ALREADY USED! Booking #{booking.Id.ToString()[..8]} was previously validated.");
        }

        try
        {
            booking.MarkAsUsed();
            await this._bookingRepository.UpdateAsync(booking);
            await this._unitOfWork.SaveChangesAsync();

            var response = MapToResponse(booking, booking.Event);
            return Result<BookingResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            return Result<BookingResponse>.Failure(ex.Message);
        }
    }

    private async Task<User> GetOrCreateUserAsync(string supabaseUserId, string email)
    {
        var user = await this._userRepository.GetBySupabaseUserIdAsync(supabaseUserId);
        if (user == null)
        {
            user = new User(supabaseUserId, email.Split('@')[0], email);
            await this._userRepository.AddAsync(user);
            await this._unitOfWork.SaveChangesAsync();
        }
        return user;
    }

    private static BookingResponse MapToResponse(Booking booking, Event @event)
    {
        return new BookingResponse(
            booking.Id,
            @event.Name,
            @event.Date,
            $"{@event.Location.City}, {@event.Location.Country}",
            booking.SelectedCategory.Name,
            booking.NumberOfSeats,
            booking.TotalPrice.Amount,
            booking.Status.ToString(),
            booking.QrCodeData,
            booking.CreatedAt
        );
    }
}

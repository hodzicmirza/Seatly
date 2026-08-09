using Microsoft.EntityFrameworkCore;
using Seatly.Domain.Entities;
using Seatly.Domain.Enums;
using Seatly.Domain.Interfaces;
using Seatly.Infrastructure.Data;

namespace Seatly.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context) => _context = context;

    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await _context
            .Bookings
            .Include(b => b.Event)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Booking>> GetAllAsync()
    {
        return await _context
            .Bookings
            .AsNoTracking()
            .Include(b => b.Event)
            .Include(b => b.User)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId)
    {
        return await _context
            .Bookings
            .AsNoTracking()
            .Include(b => b.Event)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetByEventIdAsync(Guid eventId)
    {
        return await _context
            .Bookings
            .AsNoTracking()
            .Where(b => b.EventId == eventId && b.Status != BookingStatus.Cancelled)
            .ToListAsync();
    }

    public async Task<int> GetBookedSeatsCountAsync(Guid eventId)
    {
        return await _context
            .Bookings
            .AsNoTracking()
            .Where(b => b.EventId == eventId && b.Status != BookingStatus.Cancelled)
            .SumAsync(b => b.NumberOfSeats);
    }

    public async Task<int> GetBookedSeatsCountForCategoryAsync(Guid eventId, string categoryName)
    {
        var categoryLower = categoryName.ToLower();
        return await _context
            .Bookings
            .AsNoTracking()
            .Where(b => b.EventId == eventId && b.Status != BookingStatus.Cancelled && b.SelectedCategory.Name.ToLower() == categoryLower)
            .SumAsync(b => b.NumberOfSeats);
    }

    public async Task AddAsync(Booking booking) => await _context.Bookings.AddAsync(booking);

    public Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Booking booking)
    {
        _context.Bookings.Remove(booking);
        return Task.CompletedTask;
    }
}

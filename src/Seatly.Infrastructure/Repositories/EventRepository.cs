using Microsoft.EntityFrameworkCore;
using Seatly.Domain.Entities;
using Seatly.Domain.Interfaces;
using Seatly.Infrastructure.Data;

namespace Seatly.Infrastructure.Repositories;

// Infrastructure Repository: Encapsulates EF Core operations for Event entities.
// Read operations use .AsNoTracking() to bypass Change Tracker overhead for faster query execution.
public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context) => _context = context;

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        return await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Event>> GetAllAsync()
    {
        // Read-only query: AsNoTracking avoids snapshot tracking in memory
        return await _context.Events
            .AsNoTracking()
            .OrderBy(e => e.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> SearchAsync(
        string? name,
        DateTime? from,
        DateTime? to,
        string? eventType
    )
    {
        var query = _context.Events
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(e => EF.Functions.Like(e.Name, $"%{name}%"));

        if (from.HasValue)
            query = query.Where(e => e.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.Date <= to.Value);

        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(e => EF.Property<string>(e, "EventType") == eventType);

        return await query.OrderBy(e => e.Date).ToListAsync();
    }

    public async Task AddAsync(Event @event) => await _context.Events.AddAsync(@event);

    public Task UpdateAsync(Event @event)
    {
        _context.Events.Update(@event);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var @event = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (@event != null)
            _context.Events.Remove(@event);
    }
}

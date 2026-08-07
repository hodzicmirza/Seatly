using Seatly.Domain.Entities;

namespace Seatly.Domain.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id);
    Task<IEnumerable<Event>> GetAllAsync();
    Task<IEnumerable<Event>> SearchAsync(
        string? name,
        DateTime? from,
        DateTime? to,
        string? eventType
    );
    Task AddAsync(Event @event);
    Task UpdateAsync(Event @event);
    Task DeleteAsync(Guid id);
}

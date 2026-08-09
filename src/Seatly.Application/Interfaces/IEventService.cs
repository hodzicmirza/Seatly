using Seatly.Application.Common;
using Seatly.Application.DTOs.Events;

namespace Seatly.Application.Interfaces;

public interface IEventService
{
    Task<Result<EventResponse>> CreateEventAsync(CreateEventRequest request);
    Task<Result<IEnumerable<EventResponse>>> GetAllEventsAsync();
    Task<Result<EventResponse>> GetEventByIdAsync(Guid id);
    Task<Result<IEnumerable<EventResponse>>> SearchEventsAsync(
        string? name,
        DateTime? from,
        DateTime? to,
        string? eventType
    );
    Task<Result<EventResponse>> UpdateEventAsync(Guid id, UpdateEventRequest request);
    Task<Result> DeleteEventAsync(Guid id);
}

using Microsoft.Extensions.Caching.Memory;
using Seatly.Application.Common;
using Seatly.Application.DTOs.Events;
using Seatly.Application.Interfaces;
using Seatly.Domain.Entities;
using Seatly.Domain.Enums;
using Seatly.Domain.Exceptions;
using Seatly.Domain.Factories;
using Seatly.Domain.Interfaces;
using Seatly.Domain.ValueObjects;

namespace Seatly.Application.Services;

// Application Service: Manages event creation, search, updates, deletion, and caching.
// Utilizes IMemoryCache to reduce database round-trips for high-frequency GET requests.
public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private const string AllEventsCacheKey = "AllEventsCacheKey";

    public EventService(
        IEventRepository eventRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        IMemoryCache cache
    )
    {
        _eventRepository = eventRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<EventResponse>> CreateEventAsync(CreateEventRequest request)
    {
        try
        {
            var eventDate = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
            var address = new Address(request.Street, request.City, request.Country);
            var totalCategorySeats = request.Categories.Sum(c => c.SeatsCount);
            if (totalCategorySeats > 0 && totalCategorySeats != request.TotalSeats)
            {
                return Result<EventResponse>.Failure($"The sum of category seats ({totalCategorySeats}) must equal the total event capacity ({request.TotalSeats}).");
            }

            var categories = request
                .Categories.Select(c => new SeatCategory(c.Name, c.Multiplier, c.SeatsCount))
                .ToList();
            var money = new Money(request.BasePrice);

            string? extraInfo = request.EventType == EventType.Concert
                ? (string.IsNullOrWhiteSpace(request.Headliner) ? null : request.Headliner)
                : (string.IsNullOrWhiteSpace(request.Organizer) ? null : request.Organizer);

            string? subInfo = request.EventType == EventType.Concert
                ? (string.IsNullOrWhiteSpace(request.SupportAct) ? null : request.SupportAct)
                : (string.IsNullOrWhiteSpace(request.KeynoteSpeaker) ? null : request.KeynoteSpeaker);

            // Instantiate concrete event using EventFactory pattern
            Event newEvent = EventFactory.CreateEvent(
                request.Name,
                request.Description,
                eventDate,
                address,
                money,
                request.TotalSeats,
                categories,
                request.EventType,
                extraInfo,
                subInfo
            );
            await _eventRepository.AddAsync(newEvent);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate full events list cache on creation so users immediately see the new event
            _cache.Remove(AllEventsCacheKey);

            var response = await MapToResponseAsync(newEvent);
            return Result<EventResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            return Result<EventResponse>.Failure(ex.Message);
        }
    }

    public async Task<Result<IEnumerable<EventResponse>>> GetAllEventsAsync()
    {
        // Check if event list is cached in server memory to bypass DB query
        if (!_cache.TryGetValue(AllEventsCacheKey, out IEnumerable<EventResponse>? cachedEvents) || cachedEvents == null)
        {
            var events = await _eventRepository.GetAllAsync();
            var responsesList = new List<EventResponse>();
            foreach (var @event in events)
            {
                responsesList.Add(await MapToResponseAsync(@event));
            }
            cachedEvents = responsesList;

            // Store in IMemoryCache with 30s TTL and 10s sliding expiration
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                .SetSlidingExpiration(TimeSpan.FromSeconds(10));

            _cache.Set(AllEventsCacheKey, cachedEvents, cacheEntryOptions);
        }

        return Result<IEnumerable<EventResponse>>.Success(cachedEvents);
    }

    public async Task<Result<EventResponse>> GetEventByIdAsync(Guid id)
    {
        var cacheKey = $"EventById_{id}";
        // Check single event cache entry before querying PostgreSQL
        if (!_cache.TryGetValue(cacheKey, out EventResponse? cachedEvent) || cachedEvent == null)
        {
            var @event = await _eventRepository.GetByIdAsync(id);
            if (@event == null)
            {
                return Result<EventResponse>.Failure("Event not found.");
            }
            cachedEvent = await MapToResponseAsync(@event);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30));

            _cache.Set(cacheKey, cachedEvent, cacheEntryOptions);
        }

        return Result<EventResponse>.Success(cachedEvent);
    }

    public async Task<Result<IEnumerable<EventResponse>>> SearchEventsAsync(
        string? name,
        DateTime? from,
        DateTime? to,
        string? eventType
    )
    {
        var utcFrom = from.HasValue ? DateTime.SpecifyKind(from.Value, DateTimeKind.Utc) : (DateTime?)null;
        var utcTo = to.HasValue ? DateTime.SpecifyKind(to.Value, DateTimeKind.Utc) : (DateTime?)null;

        var events = await _eventRepository.SearchAsync(name, utcFrom, utcTo, eventType);
        var responses = new List<EventResponse>();
        foreach (var @event in events)
        {
            responses.Add(await MapToResponseAsync(@event));
        }
        return Result<IEnumerable<EventResponse>>.Success(responses);
    }

    public async Task<Result<EventResponse>> UpdateEventAsync(Guid id, UpdateEventRequest request)
    {
        var @event = await _eventRepository.GetByIdAsync(id);
        if (@event == null)
        {
            return Result<EventResponse>.Failure("Event not found.");
        }

        try
        {
            var eventDate = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
            var address = new Address(request.Street, request.City, request.Country);
            var money = new Money(request.BasePrice);

            @event.UpdateDetails(request.Name, request.Description, eventDate, address, money, request.TotalSeats);
            if (request.Categories != null && request.Categories.Any())
            {
                var totalCategorySeats = request.Categories.Sum(c => c.SeatsCount);
                if (totalCategorySeats > 0 && totalCategorySeats != request.TotalSeats)
                {
                    return Result<EventResponse>.Failure($"The sum of category seats ({totalCategorySeats}) must equal the total event capacity ({request.TotalSeats}).");
                }

                var categories = request.Categories
                    .Select(c => new SeatCategory(c.Name, c.Multiplier, c.SeatsCount))
                    .ToList();
                @event.UpdateSeatCategories(categories);
            }
            await _eventRepository.UpdateAsync(@event);
            await _unitOfWork.SaveChangesAsync();

            // Evict stale cached data for this specific event and full list
            _cache.Remove(AllEventsCacheKey);
            _cache.Remove($"EventById_{id}");

            var response = await MapToResponseAsync(@event);
            return Result<EventResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            return Result<EventResponse>.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteEventAsync(Guid id)
    {
        var @event = await _eventRepository.GetByIdAsync(id);
        if (@event == null)
        {
            return Result.Failure("Event not found.");
        }

        await _eventRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        // Evict deleted event from cache
        _cache.Remove(AllEventsCacheKey);
        _cache.Remove($"EventById_{id}");

        return Result.Success();
    }

    private async Task<EventResponse> MapToResponseAsync(Event @event)
    {
        var bookedSeats = await _bookingRepository.GetBookedSeatsCountAsync(@event.Id);
        var availableSeats = @event.AvailableSeats(bookedSeats);

        return new EventResponse(
            @event.Id,
            @event.Name,
            @event.Description,
            @event.Date,
            @event.Location.City,
            @event.Location.Country,
            @event.BasePrice.Amount,
            @event.TotalSeats,
            availableSeats,
            @event.GetType().Name,
            @event
                .SeatCategories.Select(c => new CategoryResponse(
                    c.Name,
                    c.PriceMultiplier,
                    @event.CalculatePrice(c).Amount,
                    c.SeatsCount
                ))
                .ToList()
        );
    }
}

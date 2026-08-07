using Seatly.Application.Common;
using Seatly.Application.DTOs.Events;
using Seatly.Application.Interfaces;
using Seatly.Domain.Entities;
using Seatly.Domain.Enums;
using Seatly.Domain.Exceptions;
using Seatly.Domain.Interfaces;
using Seatly.Domain.ValueObjects;

namespace Seatly.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IBookingRepository _bookingRepository;
    IUnitOfWork _unitOfWork;

    public EventService(
        IEventRepository eventRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork
    )
    {
        this._eventRepository = eventRepository;
        this._bookingRepository = bookingRepository;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result<EventResponse>> CreateEventAsync(CreateEventRequest request)
    {
        try
        {
            var address = new Address(request.Street, request.City, request.Country);
            var categories = request
                .Categories.Select(c => new SeatCategory(c.Name, c.Multiplier))
                .ToList();
            var money = new Money(request.BasePrice);

            Event newEvent = request.EventType switch
            {
                EventType.Concert => new Concert(
                    request.Name,
                    request.Description,
                    request.Date,
                    address,
                    money,
                    request.TotalSeats,
                    categories,
                    request.Headliner ?? "TBA",
                    request.SupportAct
                ),

                EventType.Conference => new Conference(
                    request.Name,
                    request.Description,
                    request.Date,
                    address,
                    money,
                    request.TotalSeats,
                    categories,
                    request.Organizer ?? "TBA",
                    request.KeynoteSpeaker
                ),

                _ => throw new DomainException(
                    $"Event type {request.EventType} is not yet supported."
                ),
            };
            await this._eventRepository.AddAsync(newEvent);
            await this._unitOfWork.SaveChangesAsync();

            var respone = await MapToResponseAsync(newEvent);
            return Result<EventResponse>.Success(respone);
        }
        catch (DomainException ex)
        {
            return Result<EventResponse>.Failure(ex.Message);
        }
    }

    public async Task<Result<IEnumerable<EventResponse>>> GetAllEventsAsync()
    {
        var events = await this._eventRepository.GetAllAsync();
        var tasks = events.Select(MapToResponseAsync);
        var responses = await Task.WhenAll(tasks);
        return Result<IEnumerable<EventResponse>>.Success(responses);
    }

    public async Task<Result<EventResponse>> GetEventByIdAsync(Guid id)
    {
        var @event = await this._eventRepository.GetByIdAsync(id);
        if (@event == null)
        {
            return Result<EventResponse>.Failure("Event not found.");
        }
        var respone = await MapToResponseAsync(@event);

        return Result<EventResponse>.Success(respone);
    }

    public async Task<Result<IEnumerable<EventResponse>>> SearchEventsAsync(
        string? name,
        DateTime? from,
        DateTime? to,
        string? eventType
    )
    {
        var events = this._eventRepository.SearchAsync(name, from, to, eventType);
        var tasks = events.Select(MapToResponseAsync);
        var responses = await Task.WhenAll(tasks);
        return Result<IEnumerable<EventResponse>>.Success(responses);
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
                    @event.CalculatePrice(c).Amount
                ))
                .ToList()
        );
    }
}

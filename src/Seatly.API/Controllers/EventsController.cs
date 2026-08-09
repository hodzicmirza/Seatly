using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatly.Application.DTOs.Events;
using Seatly.Application.Interfaces;
using Seatly.Domain.Enums;

namespace Seatly.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IUserService _userService;

    public EventsController(IEventService eventService, IUserService userService)
    {
        _eventService = eventService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _eventService.GetAllEventsAsync();
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.ErrorMessage });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _eventService.GetEventByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.ErrorMessage });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? name,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? eventType
    )
    {
        var result = await _eventService.SearchEventsAsync(name, from, to, eventType);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.ErrorMessage });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
    {
        var supabaseUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(supabaseUserId))
            return Unauthorized();

        var userResult = await _userService.GetUserBySupabaseIdAsync(supabaseUserId);
        if (!userResult.IsSuccess || userResult.Value == null)
            return Unauthorized(new { error = "User not found." });

        var role = userResult.Value.Role;
        if (role != UserRole.Admin && role != UserRole.Organizer)
        {
            return StatusCode(403, new { error = "Only Admin or Organizer users can create events." });
        }

        var result = await _eventService.CreateEventAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventRequest request)
    {
        var supabaseUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(supabaseUserId))
            return Unauthorized();

        var userResult = await _userService.GetUserBySupabaseIdAsync(supabaseUserId);
        if (!userResult.IsSuccess || userResult.Value == null)
            return Unauthorized(new { error = "User not found." });

        var role = userResult.Value.Role;
        if (role != UserRole.Admin && role != UserRole.Organizer)
        {
            return StatusCode(403, new { error = "Only Admin or Organizer users can update events." });
        }

        var result = await _eventService.UpdateEventAsync(id, request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.ErrorMessage });
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var supabaseUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(supabaseUserId))
            return Unauthorized();

        var userResult = await _userService.GetUserBySupabaseIdAsync(supabaseUserId);
        if (!userResult.IsSuccess || userResult.Value == null)
            return Unauthorized(new { error = "User not found." });

        var role = userResult.Value.Role;
        if (role != UserRole.Admin && role != UserRole.Organizer)
        {
            return StatusCode(403, new { error = "Only Admin or Organizer users can delete events." });
        }

        var result = await _eventService.DeleteEventAsync(id);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.ErrorMessage });
    }
}

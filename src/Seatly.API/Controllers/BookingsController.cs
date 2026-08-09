using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatly.Application.DTOs.Bookings;
using Seatly.Application.Interfaces;

namespace Seatly.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var result = await _bookingService.GetAllBookingsAsync();
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.ErrorMessage });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        var supabaseUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(supabaseUserId))
            return Unauthorized(new { error = "User not authenticated." });

        var result = await _bookingService.CreateBookingAsync(
            request,
            supabaseUserId,
            userEmail ?? "unknown@email.com"
        );

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.ErrorMessage });
    }

    [HttpGet("my-bookings")]
    [Authorize]
    public async Task<IActionResult> GetMyBookings()
    {
        var supabaseUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(supabaseUserId))
            return Unauthorized();

        var result = await _bookingService.GetUserBookingsAsync(supabaseUserId);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.ErrorMessage });
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _bookingService.GetBookingByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.ErrorMessage });
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var supabaseUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(supabaseUserId))
            return Unauthorized();

        var result = await _bookingService.CancelBookingAsync(id, supabaseUserId);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.ErrorMessage });
    }

    [HttpDelete("{id:guid}/admin")]
    [Authorize]
    public async Task<IActionResult> DeleteAdmin(Guid id)
    {
        var result = await _bookingService.DeleteBookingAdminAsync(id);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.ErrorMessage });
    }

    [HttpPost("{id:guid}/use")]
    [Authorize]
    public async Task<IActionResult> MarkAsUsed(Guid id, [FromBody] MarkAsUsedRequest request)
    {
        var result = await _bookingService.MarkBookingAsUsedAsync(id, request.QrCodeData);
        return result.IsSuccess
            ? Ok(new { message = "Ticket marked as used" })
            : BadRequest(new { error = result.ErrorMessage });
    }
}

public record MarkAsUsedRequest(string QrCodeData);

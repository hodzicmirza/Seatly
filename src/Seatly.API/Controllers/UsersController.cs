using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seatly.Application.DTOs.Users;
using Seatly.Application.Interfaces;

namespace Seatly.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllUsersAsync();
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.ErrorMessage });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var supabaseUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(supabaseUserId))
            return Unauthorized();

        var result = await _userService.GetOrCreateUserAsync(
            supabaseUserId,
            userEmail ?? "unknown@email.com"
        );

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.ErrorMessage });
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.ErrorMessage });
    }

    [HttpPost("sync")]
    [Authorize]
    public async Task<IActionResult> SyncUser([FromBody] SyncUserRequest? request)
    {
        var supabaseUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = request?.Email ?? User.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(supabaseUserId))
            return Unauthorized();

        var result = await _userService.GetOrCreateUserAsync(
            supabaseUserId,
            userEmail ?? "unknown@email.com",
            request?.FullName
        );

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.ErrorMessage });
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        var supabaseUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(supabaseUserId))
            return Unauthorized();

        var result = await _userService.UpdateUserProfileAsync(supabaseUserId, request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.ErrorMessage });
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _userService.DeleteUserAsync(id);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.ErrorMessage });
    }
}

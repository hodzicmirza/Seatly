using Seatly.Application.Common;
using Seatly.Application.DTOs.Users;

namespace Seatly.Application.Interfaces;

public interface IUserService
{
    Task<Result<UserResponse>> GetOrCreateUserAsync(string supabaseUserId, string email, string? fullName = null);
    Task<Result<IEnumerable<UserResponse>>> GetAllUsersAsync();
    Task<Result<UserResponse>> GetUserByIdAsync(Guid id);
    Task<Result<UserResponse>> GetUserBySupabaseIdAsync(string supabaseUserId);
    Task<Result<UserResponse>> UpdateUserProfileAsync(string supabaseUserId, UpdateUserProfileRequest request);
    Task<Result> DeleteUserAsync(Guid id);
}

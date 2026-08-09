using Seatly.Application.Common;
using Seatly.Application.DTOs.Users;
using Seatly.Application.Interfaces;
using Seatly.Domain.Entities;
using Seatly.Domain.Enums;
using Seatly.Domain.Interfaces;

namespace Seatly.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserResponse>> GetOrCreateUserAsync(
        string supabaseUserId,
        string email,
        string? fullName = null
    )
    {
        var user = await _userRepository.GetBySupabaseUserIdAsync(supabaseUserId);
        if (user == null)
        {
            var name = string.IsNullOrWhiteSpace(fullName)
                ? email.Split('@')[0]
                : fullName;
            user = new User(supabaseUserId, name, email, UserRole.Customer);
            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        return Result<UserResponse>.Success(MapToResponse(user));
    }

    public async Task<Result<IEnumerable<UserResponse>>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var responses = users.Select(MapToResponse);
        return Result<IEnumerable<UserResponse>>.Success(responses);
    }

    public async Task<Result<UserResponse>> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return Result<UserResponse>.Failure("User not found.");

        return Result<UserResponse>.Success(MapToResponse(user));
    }

    public async Task<Result<UserResponse>> GetUserBySupabaseIdAsync(string supabaseUserId)
    {
        var user = await _userRepository.GetBySupabaseUserIdAsync(supabaseUserId);
        if (user == null)
            return Result<UserResponse>.Failure("User not found.");

        return Result<UserResponse>.Success(MapToResponse(user));
    }

    public async Task<Result<UserResponse>> UpdateUserProfileAsync(
        string supabaseUserId,
        UpdateUserProfileRequest request
    )
    {
        var user = await _userRepository.GetBySupabaseUserIdAsync(supabaseUserId);
        if (user == null)
            return Result<UserResponse>.Failure("User not found.");

        user.UpdateProfile(request.FullName, request.Email);
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Result<UserResponse>.Success(MapToResponse(user));
    }

    public async Task<Result> DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return Result.Failure("User not found.");

        await _userRepository.DeleteAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    private static UserResponse MapToResponse(User user)
    {
        return new UserResponse(
            user.Id,
            user.SupabaseUserId,
            user.FullName,
            user.Email,
            user.Role,
            user.CreatedAt
        );
    }
}

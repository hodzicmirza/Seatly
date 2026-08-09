using Seatly.Domain.Enums;

namespace Seatly.Application.DTOs.Users;

public record UserResponse(
    Guid Id,
    string SupabaseUserId,
    string FullName,
    string Email,
    UserRole Role,
    DateTime CreatedAt
);

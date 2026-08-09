namespace Seatly.Application.DTOs.Users;

public record SyncUserRequest(string SupabaseUserId, string Email, string? FullName);

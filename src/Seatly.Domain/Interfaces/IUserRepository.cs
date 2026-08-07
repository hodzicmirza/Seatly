using Seatly.Domain.Entities;

namespace Seatly.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetBySupabaseUserIdAsync(string supabaseUserId);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}

using Seatly.Domain.Enums;
using Seatly.Domain.Exceptions;

namespace Seatly.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string SupabaseUserId { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public DateTime CreatetAd { get; private set; }

    private User() { }

    public User(
        string supabaseUserId,
        string fullName,
        string email,
        UserRole role = UserRole.Customer
    )
    {
        if (string.IsNullOrWhiteSpace(supabaseUserId))
        {
            throw new ArgumentException("SupabaseUserId is required.");
        }
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.");
        }

        this.Id = Guid.NewGuid();
        this.SupabaseUserId = supabaseUserId;
        this.FullName = fullName;
        this.Email = email;
        this.Role = role;
        this.CreatetAd = DateTime.UtcNow;
    }

    public void UpdateProfile(string fullName, string email)
    {
        this.FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
        this.Email = email ?? throw new ArgumentNullException(nameof(email));
    }

    public void PromoteTo(UserRole newUserRole)
    {
        if (newUserRole == UserRole.Customer)
        {
            throw new ArgumentException("Cannot demote to Customer.");
        }

        this.Role = newUserRole;
    }

    public bool IsAdmin => this.Role == UserRole.Admin;
    public bool IsOrganizer => this.Role == UserRole.Organizer;
    public bool CanCreateEvents => this.Role == UserRole.Admin || this.Role == UserRole.Organizer;
}

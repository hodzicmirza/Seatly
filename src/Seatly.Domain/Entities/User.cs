namespace Seatly.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string SupabaseUserId { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Role { get; private set; } = null!; // korisiti enum kasnije
    public DateTime CreatetAd { get; private set; }

    private User() { }

    public User(string supabaseUserId, string fullName, string email, string role)
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
}

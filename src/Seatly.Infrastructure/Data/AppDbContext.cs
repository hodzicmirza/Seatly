using Microsoft.EntityFrameworkCore;
using Seatly.Domain.Entities;
using Seatly.Domain.Interfaces;

namespace Seatly.Infrastructure.Data;

// Infrastructure Layer: Entity Framework Core DbContext implementing IUnitOfWork.
// Configures TPH (Table-Per-Hierarchy) inheritance, B-Tree database indexes, and Value Object mappings.
public class AppDbContext : DbContext, IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Booking> Bookings => Set<Booking>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Event entity mappings
        modelBuilder.Entity<Event>(entity =>
        {
            // B-Tree database index on Event Date for fast chronological filtering
            entity.HasIndex(e => e.Date);

            // Table-Per-Hierarchy (TPH) inheritance mapping for Concert and Conference subclasses
            entity
                .HasDiscriminator<string>("EventTypeDiscriminator")
                .HasValue<Concert>("Concert")
                .HasValue<Conference>("Conference");

            entity.Property(e => e.EventType).HasConversion<string>().HasMaxLength(20);

            // Map Value Object: Address
            entity.OwnsOne(
                e => e.Location,
                location =>
                {
                    location.Property(l => l.Street).HasColumnName("Location_Street");
                    location.Property(l => l.City).HasColumnName("Location_City");
                    location.Property(l => l.Country).HasColumnName("Location_Country");
                }
            );

            // Map Value Object: Money
            entity.OwnsOne(
                e => e.BasePrice,
                money =>
                {
                    money.Property(m => m.Amount).HasColumnName("BasePrice_Amount");
                    money.Property(m => m.Currency).HasColumnName("BasePrice_Currency");
                }
            );

            // Map owned collection of SeatCategory value objects
            entity.OwnsMany(
                e => e.SeatCategories,
                cat =>
                {
                    cat.WithOwner().HasForeignKey("EventId");
                    cat.Property<Guid>("Id");
                    cat.HasKey("Id");
                    cat.Property(c => c.SeatsCount).HasDefaultValue(0);
                }
            );
        });

        // Configure Booking entity mappings and foreign key performance indexes
        modelBuilder.Entity<Booking>(entity =>
        {
            // High-throughput B-Tree indexes for foreign keys and booking status filtering
            entity.HasIndex(b => b.UserId);
            entity.HasIndex(b => b.EventId);
            entity.HasIndex(b => b.Status);

            entity
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(b => b.Event)
                .WithMany()
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.OwnsOne(
                b => b.TotalPrice,
                money =>
                {
                    money.Property(m => m.Amount).HasColumnName("TotalPrice_Amount");
                    money.Property(m => m.Currency).HasColumnName("TotalPrice_Currency");
                }
            );

            entity.OwnsOne(
                b => b.SelectedCategory,
                cat =>
                {
                    cat.Property(c => c.Name).HasColumnName("Category_Name");
                    cat.Property(c => c.PriceMultiplier).HasColumnName("Category_Multiplier");
                    cat.Property(c => c.SeatsCount).HasColumnName("Category_SeatsCount").HasDefaultValue(0);
                }
            );

            entity.Property(b => b.QrCodeData).HasColumnType("text");
        });

        // Configure User entity mappings
        modelBuilder.Entity<User>(entity =>
        {
            // Unique indexes for Supabase Auth identity lookup
            entity.HasIndex(u => u.SupabaseUserId).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
        });
    }
}

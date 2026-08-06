using Seatly.Domain.Enums;
using Seatly.Domain.Exceptions;
using Seatly.Domain.ValueObjects;

namespace Seatly.Domain.Entities;

public class Booking
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid EventId { get; private set; }

    public int NumberOfSeats { get; private set; }

    public SeatCategory SelectedCategory { get; private set; }
    public Money TotalPrice { get; private set; }
    public BookingStatus Status { get; private set; }
    public string? QrCodeData { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ConfirmedAt { get; private set; }
    public DateTime CancelledAt { get; private set; }

    public User User { get; private set; } = null!;
    public Event Event { get; private set; } = null!;

    private Booking() { }

    public Booking(
        Guid userId,
        Guid eventId,
        int numberOfSeats,
        SeatCategory selectedCategory,
        Money totalPrice
    )
    {
        if (numberOfSeats <= 0)
        {
            throw new DomainException("Number of seats must be greater than 0.");
        }

        this.Id = Guid.NewGuid();
        this.UserId = userId;
        this.EventId = eventId;
        this.NumberOfSeats = numberOfSeats;
        this.SelectedCategory = selectedCategory;
        this.TotalPrice = totalPrice;
        this.Status = BookingStatus.Pending;
        this.CreatedAt = DateTime.UtcNow;
    }

    public void Confirm(string qrCodeData)
    {
        if (this.Status != BookingStatus.Pending)
        {
            throw new DomainException("Only pending bookings can be confirmed.");
        }

        if (string.IsNullOrWhiteSpace(qrCodeData))
        {
            throw new DomainException("QR code data is required for confirmation.");
        }

        this.Status = BookingStatus.Confirmed;
        this.QrCodeData = qrCodeData;
        this.CreatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (this.Status == BookingStatus.Cancelled)
        {
            throw new DomainException("Booking is already cancelled.");
        }

        if (this.Status == BookingStatus.Used)
        {
            throw new DomainException("Cannot cancel a used booking.");
        }

        this.Status = BookingStatus.Cancelled;
        this.CancelledAt = DateTime.UtcNow;
    }

    public void MarkAsUsed()
    {
        if (this.Status != BookingStatus.Confirmed)
        {
            throw new DomainException("Only confirmed bookings can be marked as used.");
        }

        this.Status = BookingStatus.Used;
    }
}

namespace Seatly.Domain.ValueObjects;

public record Money(decimal Ammout, string Currency = "BAM")
{
    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
        {
            throw new InvalidOperationException("Cannot add money with different currencies.");
        }

        return a with
        {
            Ammout = a.Ammout + b.Ammout,
        };
    }

    public static Money operator *(Money a, decimal multiplier)
    {
        return a with { Ammout = Math.Round(a.Ammout * multiplier, 2) };
    }

    public override string ToString() => $"{Ammout:F2} {Currency}";
}

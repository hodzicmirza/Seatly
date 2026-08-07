namespace Seatly.Domain.ValueObjects;

public record Money(decimal Amount, string Currency = "BAM")
{
    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
        {
            throw new InvalidOperationException("Cannot add money with different currencies.");
        }

        return a with
        {
            Amount = a.Amount + b.Amount,
        };
    }

    public static Money operator *(Money a, decimal multiplier)
    {
        return a with { Amount = Math.Round(a.Amount * multiplier, 2) };
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}

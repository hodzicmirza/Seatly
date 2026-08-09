using Seatly.Application.Services.Discounts;
using Seatly.Domain.ValueObjects;
using Xunit;

namespace Seatly.Tests;

public class DiscountStrategyTests
{
    [Fact]
    public void BulkDiscount_ShouldApplyFifteenPercentDiscount()
    {
        // Arrange
        var bulkDiscount = new BulkDiscount();
        var originalPrice = new Money(100m, "BAM");

        // Act
        var discountedPrice = bulkDiscount.ApplyDiscount(originalPrice);

        // Assert
        Assert.Equal(85m, discountedPrice.Amount);
        Assert.Equal("BAM", discountedPrice.Currency);
    }

    [Fact]
    public void EarlyBirdDiscount_ShouldApplyTenPercentDiscount()
    {
        // Arrange
        var earlyBirdDiscount = new EarlyBirdDiscount();
        var originalPrice = new Money(100m, "BAM");

        // Act
        var discountedPrice = earlyBirdDiscount.ApplyDiscount(originalPrice);

        // Assert
        Assert.Equal(90m, discountedPrice.Amount);
        Assert.Equal("BAM", discountedPrice.Currency);
    }

    [Fact]
    public void VIPDiscount_ShouldApplyTwentyPercentDiscount()
    {
        // Arrange
        var vipDiscount = new VIPDiscount();
        var originalPrice = new Money(100m, "BAM");

        // Act
        var discountedPrice = vipDiscount.ApplyDiscount(originalPrice);

        // Assert
        Assert.Equal(80m, discountedPrice.Amount);
        Assert.Equal("BAM", discountedPrice.Currency);
    }
}

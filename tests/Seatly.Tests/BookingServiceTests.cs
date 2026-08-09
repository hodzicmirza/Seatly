using Moq;
using Seatly.Application.Common;
using Seatly.Application.DTOs.Bookings;
using Seatly.Application.Interfaces;
using Seatly.Application.Services;
using Seatly.Application.Services.Discounts;
using Seatly.Domain.Entities;
using Seatly.Domain.Enums;
using Seatly.Domain.Interfaces;
using Seatly.Domain.ValueObjects;
using Xunit;

namespace Seatly.Tests;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<IEventRepository> _eventRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IQrCodeService> _qrCodeMock = new();
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly List<IDiscountStrategy> _discountStrategies = new()
    {
        new BulkDiscount(),
        new EarlyBirdDiscount(),
        new VIPDiscount()
    };

    [Fact]
    public async Task CreateBookingAsync_ShouldReturnFailure_WhenEventNotFound()
    {
        // Arrange
        _eventRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Event?)null);

        var service = new BookingService(
            _bookingRepoMock.Object,
            _eventRepoMock.Object,
            _userRepoMock.Object,
            _unitOfWorkMock.Object,
            _discountStrategies,
            _qrCodeMock.Object,
            _emailMock.Object
        );

        var request = new CreateBookingRequest(Guid.NewGuid(), 2, "VIP");

        // Act
        var result = await service.CreateBookingAsync(request, "sub-123", "test@seatly.app");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Event not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldFail_WhenCategoryCapacityExceeded()
    {
        // Arrange
        var @event = Seatly.Domain.Factories.EventFactory.CreateEvent(
            "Test Concert",
            "Description",
            DateTime.UtcNow.AddDays(10),
            new Address("Street", "City", "Country"),
            new Money(100),
            20,
            new List<SeatCategory>
            {
                new SeatCategory("VIP", 2.0m, 5),
                new SeatCategory("Standard", 1.0m, 15)
            },
            Seatly.Domain.Enums.EventType.Concert,
            "Headliner",
            "SupportAct"
        );

        _eventRepoMock.Setup(r => r.GetByIdAsync(@event.Id)).ReturnsAsync(@event);
        _userRepoMock.Setup(r => r.GetBySupabaseUserIdAsync("sub-123")).ReturnsAsync(new User("sub-123", "Test User", "test@seatly.app"));
        _bookingRepoMock.Setup(r => r.GetBookedSeatsCountAsync(@event.Id)).ReturnsAsync(0);
        _bookingRepoMock.Setup(r => r.GetBookedSeatsCountForCategoryAsync(@event.Id, "VIP")).ReturnsAsync(4);

        var service = new BookingService(
            _bookingRepoMock.Object,
            _eventRepoMock.Object,
            _userRepoMock.Object,
            _unitOfWorkMock.Object,
            _discountStrategies,
            _qrCodeMock.Object,
            _emailMock.Object
        );

        var request = new CreateBookingRequest(@event.Id, 2, "VIP"); // 4 booked + 2 requested = 6 > 5 max!

        // Act
        var result = await service.CreateBookingAsync(request, "sub-123", "test@seatly.app");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Not enough seats available in category 'VIP'", result.ErrorMessage);
    }
}

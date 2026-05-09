using Ambev.DeveloperEvaluation.Domain.Events;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Events;

/// <summary>
/// Contains unit tests for domain event classes.
/// Verifies that all event properties are correctly set upon construction.
/// </summary>
public class SaleEventsTests
{
    [Fact(DisplayName = "Given SaleCreatedEvent When created Then properties are set correctly")]
    public void Given_SaleCreatedEvent_When_Created_Then_PropertiesAreSet()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var saleNumber = "SALE-001";
        var customerId = Guid.NewGuid();
        var totalAmount = 500m;

        // Act
        var @event = new SaleCreatedEvent(saleId, saleNumber, customerId, totalAmount);

        // Assert
        @event.SaleId.Should().Be(saleId);
        @event.SaleNumber.Should().Be(saleNumber);
        @event.CustomerId.Should().Be(customerId);
        @event.TotalAmount.Should().Be(totalAmount);
        @event.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "Given SaleModifiedEvent When created Then properties are set correctly")]
    public void Given_SaleModifiedEvent_When_Created_Then_PropertiesAreSet()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var saleNumber = "SALE-001";
        var totalAmount = 600m;

        // Act
        var @event = new SaleModifiedEvent(saleId, saleNumber, totalAmount);

        // Assert
        @event.SaleId.Should().Be(saleId);
        @event.SaleNumber.Should().Be(saleNumber);
        @event.TotalAmount.Should().Be(totalAmount);
        @event.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "Given SaleCancelledEvent When created Then properties are set correctly")]
    public void Given_SaleCancelledEvent_When_Created_Then_PropertiesAreSet()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var saleNumber = "SALE-001";

        // Act
        var @event = new SaleCancelledEvent(saleId, saleNumber);

        // Assert
        @event.SaleId.Should().Be(saleId);
        @event.SaleNumber.Should().Be(saleNumber);
        @event.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "Given ItemCancelledEvent When created Then properties are set correctly")]
    public void Given_ItemCancelledEvent_When_Created_Then_PropertiesAreSet()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        // Act
        var @event = new ItemCancelledEvent(saleId, itemId, productId);

        // Assert
        @event.SaleId.Should().Be(saleId);
        @event.ItemId.Should().Be(itemId);
        @event.ProductId.Should().Be(productId);
        @event.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}

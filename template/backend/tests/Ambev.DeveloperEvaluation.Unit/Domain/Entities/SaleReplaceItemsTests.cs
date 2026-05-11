using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Contains additional unit tests for Sale entity covering ReplaceItems
/// and SaleItem cancellation behavior.
/// </summary>
public class SaleReplaceItemsTests
{
    [Fact(DisplayName = "Given sale with items When replacing items Then old items are cleared and new ones applied")]
    public void Given_SaleWithItems_When_ReplacingItems_Then_OldItemsCleared()
    {
        // Arrange
        var sale = new Sale
        {
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch"
        };
        sale.AddItem(SaleTestData.GenerateValidSaleItem(quantity: 3, unitPrice: 50m));

        var newItems = new List<SaleItem>
        {
            SaleTestData.GenerateValidSaleItem(quantity: 5, unitPrice: 100m),
            SaleTestData.GenerateValidSaleItem(quantity: 10, unitPrice: 200m)
        };

        // Act
        sale.ReplaceItems(newItems);

        // Assert
        sale.Items.Should().HaveCount(2);
        sale.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Given new items with 5 units When replacing Then 10% discount applied")]
    public void Given_NewItemsWithFiveUnits_When_Replacing_Then_TenPercentDiscountApplied()
    {
        // Arrange
        var sale = new Sale
        {
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch"
        };

        var newItems = new List<SaleItem>
        {
            SaleTestData.GenerateValidSaleItem(quantity: 5, unitPrice: 100m)
        };

        // Act
        sale.ReplaceItems(newItems);

        // Assert
        sale.Items.First().Discount.Should().Be(0.10m);
        sale.TotalAmount.Should().Be(450m); // 5 * 100 * 0.90
    }

    [Fact(DisplayName = "Given new items exceeding max quantity When replacing Then throws DomainException")]
    public void Given_NewItemsExceedingMaxQuantity_When_Replacing_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = new Sale
        {
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch"
        };

        var newItems = new List<SaleItem>
        {
            SaleTestData.GenerateValidSaleItem(quantity: 21, unitPrice: 100m)
        };

        // Act
        var act = () => sale.ReplaceItems(newItems);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot sell more than 20 identical items*");
    }

    [Fact(DisplayName = "Given sale with multiple items When recalculating Then total excludes cancelled items")]
    public void Given_SaleWithMultipleItems_When_Recalculating_Then_TotalExcludesCancelled()
    {
        // Arrange
        var sale = new Sale
        {
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch"
        };

        var item1 = SaleTestData.GenerateValidSaleItem(quantity: 3, unitPrice: 100m);
        var item2 = SaleTestData.GenerateValidSaleItem(quantity: 3, unitPrice: 100m);
        sale.AddItem(item1);
        sale.AddItem(item2);

        // Act
        sale.CancelItem(item1.Id);

        // Assert
        sale.TotalAmount.Should().Be(300m); // only item2: 3 * 100 * 1.0 (no discount < 4)
    }

    [Fact(DisplayName = "Given SaleItem When applying discount below 4 items Then throws DomainException")]
    public void Given_SaleItem_When_ApplyingDiscountBelowFour_Then_ThrowsDomainException()
    {
        // Arrange
        var item = new SaleItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Product",
            Quantity = 3,
            UnitPrice = 100m
        };

        // Act
        var act = () => item.ApplyDiscount(0.10m);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Discount cannot be applied to fewer than 4 identical items*");
    }

    [Fact(DisplayName = "Given SaleItem When cancelled Then IsCancelled is true")]
    public void Given_SaleItem_When_Cancelled_Then_IsCancelledIsTrue()
    {
        // Arrange
        var item = SaleTestData.GenerateValidSaleItem(quantity: 5, unitPrice: 100m);

        // Act
        item.Cancel();

        // Assert
        item.IsCancelled.Should().BeTrue();
    }
}

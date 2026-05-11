using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Contains unit tests for the Sale entity class.
/// Tests cover business rules: discount tiers, quantity limits, cancellation and total calculation.
/// </summary>
public class SaleTests
{
    // ── Discount Rules ────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given quantity below 4 When calculating discount Then returns 0%")]
    public void Given_QuantityBelowFour_When_CalculatingDiscount_Then_ReturnsZero()
    {
        // Arrange
        var quantities = new[] { 1, 2, 3 };

        // Act & Assert
        foreach (var qty in quantities)
            Sale.CalculateDiscount(qty).Should().Be(0m,
                because: $"quantity {qty} is below the minimum for any discount");
    }

    [Fact(DisplayName = "Given quantity between 4 and 9 When calculating discount Then returns 10%")]
    public void Given_QuantityBetweenFourAndNine_When_CalculatingDiscount_Then_ReturnsTenPercent()
    {
        // Arrange
        var quantities = new[] { 4, 5, 6, 7, 8, 9 };

        // Act & Assert
        foreach (var qty in quantities)
            Sale.CalculateDiscount(qty).Should().Be(0.10m,
                because: $"quantity {qty} is in the 10% discount tier");
    }

    [Fact(DisplayName = "Given quantity between 10 and 20 When calculating discount Then returns 20%")]
    public void Given_QuantityBetweenTenAndTwenty_When_CalculatingDiscount_Then_ReturnsTwentyPercent()
    {
        // Arrange
        var quantities = new[] { 10, 11, 15, 19, 20 };

        // Act & Assert
        foreach (var qty in quantities)
            Sale.CalculateDiscount(qty).Should().Be(0.20m,
                because: $"quantity {qty} is in the 20% discount tier");
    }

    // ── Quantity Limits ───────────────────────────────────────────────────────

    [Fact(DisplayName = "Given quantity above 20 When adding item Then throws DomainException")]
    public void Given_QuantityAboveTwenty_When_AddingItem_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = new Sale
        {
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Test Branch"
        };
        var item = SaleTestData.GenerateItemExceedingMaxQuantity();

        // Act
        var act = () => sale.AddItem(item);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot sell more than 20 identical items*");
    }

    [Fact(DisplayName = "Given quantity of exactly 20 When adding item Then succeeds with 20% discount")]
    public void Given_QuantityOfTwenty_When_AddingItem_Then_SucceedsWithTwentyPercentDiscount()
    {
        // Arrange
        var sale = new Sale
        {
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Test Branch"
        };
        var item = SaleTestData.GenerateValidSaleItem(quantity: 20, unitPrice: 100m);

        // Act
        sale.AddItem(item);

        // Assert
        sale.Items.Should().HaveCount(1);
        sale.Items.First().Discount.Should().Be(0.20m);
    }

    [Fact(DisplayName = "Given zero quantity When adding item Then throws DomainException")]
    public void Given_ZeroQuantity_When_AddingItem_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = new Sale
        {
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Test Branch"
        };
        var item = SaleTestData.GenerateValidSaleItem(quantity: 0);

        // Act
        var act = () => sale.AddItem(item);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Item quantity must be greater than zero*");
    }

    // ── Total Amount Calculation ──────────────────────────────────────────────

    [Fact(DisplayName = "Given item with 5 units at 100 each When adding Then total is 450 after 10% discount")]
    public void Given_FiveItemsAtHundred_When_Added_Then_TotalIsFourFifty()
    {
        // Arrange
        var sale = new Sale
        {
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Test Branch"
        };
        var item = SaleTestData.GenerateValidSaleItem(quantity: 5, unitPrice: 100m);

        // Act
        sale.AddItem(item);

        // Assert
        sale.TotalAmount.Should().Be(450m); // 5 * 100 * 0.90
    }

    [Fact(DisplayName = "Given item with 10 units at 100 each When adding Then total is 800 after 20% discount")]
    public void Given_TenItemsAtHundred_When_Added_Then_TotalIsEightHundred()
    {
        // Arrange
        var sale = new Sale
        {
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Test Branch"
        };
        var item = SaleTestData.GenerateValidSaleItem(quantity: 10, unitPrice: 100m);

        // Act
        sale.AddItem(item);

        // Assert
        sale.TotalAmount.Should().Be(800m); // 10 * 100 * 0.80
    }

    // ── Cancellation ──────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given active sale When cancelled Then IsCancelled is true and all items are cancelled")]
    public void Given_ActiveSale_When_Cancelled_Then_IsCancelledIsTrue()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale(quantity: 5);

        // Act
        sale.Cancel();

        // Assert
        sale.IsCancelled.Should().BeTrue();
        sale.Items.Should().AllSatisfy(i => i.IsCancelled.Should().BeTrue());
        sale.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Given sale with item When item cancelled Then total is recalculated")]
    public void Given_SaleWithItem_When_ItemCancelled_Then_TotalIsRecalculated()
    {
        // Arrange
        var sale = new Sale
        {
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Test Branch"
        };

        var item1 = SaleTestData.GenerateValidSaleItem(quantity: 5, unitPrice: 100m);
        var item2 = SaleTestData.GenerateValidSaleItem(quantity: 5, unitPrice: 100m);
        sale.AddItem(item1);
        sale.AddItem(item2);

        var totalBefore = sale.TotalAmount;

        // Act
        sale.CancelItem(item1.Id);

        // Assert
        sale.TotalAmount.Should().BeLessThan(totalBefore);
        sale.Items.First(i => i.Id == item1.Id).IsCancelled.Should().BeTrue();
        sale.Items.First(i => i.Id == item2.Id).IsCancelled.Should().BeFalse();
    }

    [Fact(DisplayName = "Given sale When cancelling non-existent item Then throws DomainException")]
    public void Given_Sale_When_CancellingNonExistentItem_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale(quantity: 5);
        var nonExistentItemId = Guid.NewGuid();

        // Act
        var act = () => sale.CancelItem(nonExistentItemId);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"*{nonExistentItemId}*");
    }

    // ── Validation ────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given valid sale data When validated Then returns valid result")]
    public void Given_ValidSaleData_When_Validated_Then_ReturnsValid()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale(quantity: 5);

        // Act
        var result = sale.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}

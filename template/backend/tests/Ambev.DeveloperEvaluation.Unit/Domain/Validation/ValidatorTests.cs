using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

/// <summary>
/// Contains unit tests for the SaleValidator class.
/// </summary>
public class SaleValidatorTests
{
    private readonly SaleValidator _validator = new();

    [Fact(DisplayName = "Given valid sale When validating Then returns valid")]
    public void Given_ValidSale_When_Validating_Then_ReturnsValid()
    {
        var sale = SaleTestData.GenerateValidSale(quantity: 5);
        var result = _validator.Validate(sale);
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Given sale with empty sale number When validating Then returns invalid")]
    public void Given_SaleWithEmptySaleNumber_When_Validating_Then_ReturnsInvalid()
    {
        var sale = SaleTestData.GenerateValidSale(quantity: 5);
        sale.SaleNumber = string.Empty;
        var result = _validator.Validate(sale);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SaleNumber");
    }

    [Fact(DisplayName = "Given sale with empty customer ID When validating Then returns invalid")]
    public void Given_SaleWithEmptyCustomerId_When_Validating_Then_ReturnsInvalid()
    {
        var sale = SaleTestData.GenerateValidSale(quantity: 5);
        sale.CustomerId = Guid.Empty;
        var result = _validator.Validate(sale);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
    }

    [Fact(DisplayName = "Given sale with empty branch ID When validating Then returns invalid")]
    public void Given_SaleWithEmptyBranchId_When_Validating_Then_ReturnsInvalid()
    {
        var sale = SaleTestData.GenerateValidSale(quantity: 5);
        sale.BranchId = Guid.Empty;
        var result = _validator.Validate(sale);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BranchId");
    }
}

/// <summary>
/// Contains unit tests for the SaleItemValidator class.
/// </summary>
public class SaleItemValidatorTests
{
    private readonly SaleItemValidator _validator = new();

    [Fact(DisplayName = "Given valid sale item When validating Then returns valid")]
    public void Given_ValidSaleItem_When_Validating_Then_ReturnsValid()
    {
        var item = new SaleItem
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Valid Product",
            Quantity = 5,
            UnitPrice = 100m
        };
        item.ApplyDiscount(0.10m);
        var result = _validator.Validate(item);
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Given sale item with empty product ID When validating Then returns invalid")]
    public void Given_SaleItemWithEmptyProductId_When_Validating_Then_ReturnsInvalid()
    {
        var item = new SaleItem { ProductId = Guid.Empty, ProductName = "Product", Quantity = 5, UnitPrice = 100m };
        var result = _validator.Validate(item);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
    }

    [Fact(DisplayName = "Given sale item with zero quantity When validating Then returns invalid")]
    public void Given_SaleItemWithZeroQuantity_When_Validating_Then_ReturnsInvalid()
    {
        var item = new SaleItem { ProductId = Guid.NewGuid(), ProductName = "Product", Quantity = 0, UnitPrice = 100m };
        var result = _validator.Validate(item);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
    }

    [Fact(DisplayName = "Given sale item with quantity above 20 When validating Then returns invalid")]
    public void Given_SaleItemWithQuantityAbove20_When_Validating_Then_ReturnsInvalid()
    {
        var item = new SaleItem { ProductId = Guid.NewGuid(), ProductName = "Product", Quantity = 21, UnitPrice = 100m };
        var result = _validator.Validate(item);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
    }

    [Fact(DisplayName = "Given sale item with zero unit price When validating Then returns invalid")]
    public void Given_SaleItemWithZeroUnitPrice_When_Validating_Then_ReturnsInvalid()
    {
        var item = new SaleItem { ProductId = Guid.NewGuid(), ProductName = "Product", Quantity = 5, UnitPrice = 0m };
        var result = _validator.Validate(item);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UnitPrice");
    }

    [Fact(DisplayName = "Given sale item with empty product name When validating Then returns invalid")]
    public void Given_SaleItemWithEmptyProductName_When_Validating_Then_ReturnsInvalid()
    {
        var item = new SaleItem { ProductId = Guid.NewGuid(), ProductName = string.Empty, Quantity = 5, UnitPrice = 100m };
        var result = _validator.Validate(item);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductName");
    }
}

/// <summary>
/// Contains unit tests for the CartItemValidator class.
/// </summary>
public class CartItemValidatorTests
{
    private readonly CartItemValidator _validator = new();

    [Fact(DisplayName = "Given valid cart item When validating Then returns valid")]
    public void Given_ValidCartItem_When_Validating_Then_ReturnsValid()
    {
        var item = CartItem.Create(Guid.NewGuid(), 3);
        var result = _validator.Validate(item);
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Given cart item with empty product ID When validating Then returns invalid")]
    public void Given_CartItemWithEmptyProductId_When_Validating_Then_ReturnsInvalid()
    {
        var item = new CartItem { ProductId = Guid.Empty };
        var result = _validator.Validate(item);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
    }
}

/// <summary>
/// Contains unit tests for the CartValidator class.
/// </summary>
public class CartValidatorTests
{
    private readonly CartValidator _validator = new();

    [Fact(DisplayName = "Given valid cart When validating Then returns valid")]
    public void Given_ValidCart_When_Validating_Then_ReturnsValid()
    {
        var cart = CartTestData.GenerateValidCart();
        var result = _validator.Validate(cart);
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Given cart without user When validating Then returns invalid")]
    public void Given_CartWithoutUser_When_Validating_Then_ReturnsInvalid()
    {
        var cart = new Cart { UserId = Guid.Empty, Date = DateTime.UtcNow };
        cart.AddItem(CartItem.Create(Guid.NewGuid(), 1));
        var result = _validator.Validate(cart);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }
}

/// <summary>
/// Contains unit tests for the ProductValidator class.
/// </summary>
public class ProductValidatorTests
{
    private readonly ProductValidator _validator = new();

    [Fact(DisplayName = "Given valid product When validating Then returns valid")]
    public void Given_ValidProduct_When_Validating_Then_ReturnsValid()
    {
        var product = ProductTestData.GenerateValidProduct();
        var result = _validator.Validate(product);
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Given product with rating above 5 When validating Then returns invalid")]
    public void Given_ProductWithRatingAboveFive_When_Validating_Then_ReturnsInvalid()
    {
        var product = ProductTestData.GenerateValidProduct();
        product.Rating.Rate = 5.1m;
        var result = _validator.Validate(product);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Rating.Rate");
    }

    [Fact(DisplayName = "Given product with negative count When validating Then returns invalid")]
    public void Given_ProductWithNegativeCount_When_Validating_Then_ReturnsInvalid()
    {
        var product = ProductTestData.GenerateValidProduct();
        product.Rating.Count = -1;
        var result = _validator.Validate(product);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Rating.Count");
    }
}

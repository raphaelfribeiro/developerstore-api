using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Contains unit tests for the Cart entity class.
/// </summary>
public class CartTests
{
    [Fact(DisplayName = "Given valid cart data When validated Then returns valid result")]
    public void Given_ValidCartData_When_Validated_Then_ReturnsValid()
    {
        // Arrange
        var cart = CartTestData.GenerateValidCart();

        // Act
        var result = cart.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Given new product When adding to cart Then cart has one item")]
    public void Given_NewProduct_When_AddingToCart_Then_CartHasOneItem()
    {
        // Arrange
        var cart = new Cart { UserId = Guid.NewGuid(), Date = DateTime.UtcNow };
        var item = CartItem.Create(Guid.NewGuid(), 3);

        // Act
        cart.AddItem(item);

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.Items.First().Quantity.Should().Be(3);
    }

    [Fact(DisplayName = "Given existing product When adding again Then quantity is updated")]
    public void Given_ExistingProduct_When_AddingAgain_Then_QuantityIsUpdated()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var cart = new Cart { UserId = Guid.NewGuid(), Date = DateTime.UtcNow };
        cart.AddItem(CartItem.Create(productId, 2));

        // Act
        cart.AddItem(CartItem.Create(productId, 5));

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.Items.First().Quantity.Should().Be(5);
    }

    [Fact(DisplayName = "Given cart with items When updating items Then old items are replaced")]
    public void Given_CartWithItems_When_UpdatingItems_Then_OldItemsAreReplaced()
    {
        // Arrange
        var cart = CartTestData.GenerateValidCart();
        var newItems = new List<CartItem>
        {
            CartItem.Create(Guid.NewGuid(), 10),
            CartItem.Create(Guid.NewGuid(), 5)
        };

        // Act
        cart.UpdateItems(newItems);

        // Assert
        cart.Items.Should().HaveCount(2);
        cart.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Given zero quantity When creating CartItem Then throws DomainException")]
    public void Given_ZeroQuantity_When_CreatingCartItem_Then_ThrowsDomainException()
    {
        // Act
        var act = () => CartItem.Create(Guid.NewGuid(), 0);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Cart item quantity must be greater than zero*");
    }

    [Fact(DisplayName = "Given cart without user When validated Then returns invalid")]
    public void Given_CartWithoutUser_When_Validated_Then_ReturnsInvalid()
    {
        // Arrange
        var cart = new Cart
        {
            UserId = Guid.Empty,
            Date = DateTime.UtcNow
        };
        cart.AddItem(CartItem.Create(Guid.NewGuid(), 1));

        // Act
        var result = cart.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}

/// <summary>
/// Contains unit tests for the Product entity class.
/// </summary>
public class ProductTests
{
    [Fact(DisplayName = "Given valid product data When validated Then returns valid result")]
    public void Given_ValidProductData_When_Validated_Then_ReturnsValid()
    {
        // Arrange
        var product = ProductTestData.GenerateValidProduct();

        // Act
        var result = product.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Given product with empty title When validated Then returns invalid")]
    public void Given_ProductWithEmptyTitle_When_Validated_Then_ReturnsInvalid()
    {
        // Arrange
        var product = ProductTestData.GenerateValidProduct();
        product.Title = string.Empty;

        // Act
        var result = product.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "Given product with negative price When validated Then returns invalid")]
    public void Given_ProductWithNegativePrice_When_Validated_Then_ReturnsInvalid()
    {
        // Arrange
        var product = ProductTestData.GenerateValidProduct();
        product.Price = -1m;

        // Act
        var result = product.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "Given product with rating above 5 When validated Then returns invalid")]
    public void Given_ProductWithRatingAboveFive_When_Validated_Then_ReturnsInvalid()
    {
        // Arrange
        var product = ProductTestData.GenerateValidProduct();
        product.Rating.Rate = 5.1m;

        // Act
        var result = product.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}

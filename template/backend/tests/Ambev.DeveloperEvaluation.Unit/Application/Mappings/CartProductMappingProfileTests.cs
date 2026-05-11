using Ambev.DeveloperEvaluation.Application.Carts.CreateCart;
using Ambev.DeveloperEvaluation.Application.Carts.GetCart;
using Ambev.DeveloperEvaluation.Application.Carts.GetCarts;
using Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;
using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Application.Products.GetProduct;
using Ambev.DeveloperEvaluation.Application.Products.GetProducts;
using Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Mappings;

public class CartMappingProfileTests
{
    private readonly IMapper _mapper;

    public CartMappingProfileTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CreateCartProfile>();
            cfg.AddProfile<GetCartProfile>();
            cfg.AddProfile<GetCartsProfile>();
            cfg.AddProfile<UpdateCartProfile>();
        }, NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact(DisplayName = "Given Cart When mapping to CreateCartResult Then maps correctly")]
    public void Given_Cart_When_MappingToCreateCartResult_Then_MapsCorrectly()
    {
        var cart = CartTestData.GenerateValidCart();
        var result = _mapper.Map<CreateCartResult>(cart);
        result.Should().NotBeNull();
        result.UserId.Should().Be(cart.UserId);
        result.Items.Should().HaveCount(cart.Items.Count);
    }

    [Fact(DisplayName = "Given CartItem When mapping to CreateCartItemResult Then maps correctly")]
    public void Given_CartItem_When_MappingToCreateCartItemResult_Then_MapsCorrectly()
    {
        var item = CartTestData.GenerateValidCartItem(quantity: 3);
        var result = _mapper.Map<CreateCartItemResult>(item);
        result.Should().NotBeNull();
        result.ProductId.Should().Be(item.ProductId);
        result.Quantity.Should().Be(item.Quantity);
    }

    [Fact(DisplayName = "Given Cart When mapping to GetCartResult Then maps correctly")]
    public void Given_Cart_When_MappingToGetCartResult_Then_MapsCorrectly()
    {
        var cart = CartTestData.GenerateValidCart();
        var result = _mapper.Map<GetCartResult>(cart);
        result.Should().NotBeNull();
        result.UserId.Should().Be(cart.UserId);
    }

    [Fact(DisplayName = "Given CartItem When mapping to GetCartItemResult Then maps correctly")]
    public void Given_CartItem_When_MappingToGetCartItemResult_Then_MapsCorrectly()
    {
        var item = CartTestData.GenerateValidCartItem(quantity: 5);
        var result = _mapper.Map<GetCartItemResult>(item);
        result.Should().NotBeNull();
        result.ProductId.Should().Be(item.ProductId);
        result.Quantity.Should().Be(item.Quantity);
    }

    [Fact(DisplayName = "Given Cart When mapping to GetCartsResult Then maps correctly")]
    public void Given_Cart_When_MappingToGetCartsResult_Then_MapsCorrectly()
    {
        var cart = CartTestData.GenerateValidCart();
        var result = _mapper.Map<GetCartsResult>(cart);
        result.Should().NotBeNull();
        result.UserId.Should().Be(cart.UserId);
        result.ItemCount.Should().Be(cart.Items.Count);
    }

    [Fact(DisplayName = "Given Cart When mapping to UpdateCartResult Then maps correctly")]
    public void Given_Cart_When_MappingToUpdateCartResult_Then_MapsCorrectly()
    {
        var cart = CartTestData.GenerateValidCart();
        var result = _mapper.Map<UpdateCartResult>(cart);
        result.Should().NotBeNull();
        result.UserId.Should().Be(cart.UserId);
    }

    [Fact(DisplayName = "Given CartItem When mapping to UpdateCartItemResult Then maps correctly")]
    public void Given_CartItem_When_MappingToUpdateCartItemResult_Then_MapsCorrectly()
    {
        var item = CartTestData.GenerateValidCartItem(quantity: 3);
        var result = _mapper.Map<UpdateCartItemResult>(item);
        result.Should().NotBeNull();
        result.ProductId.Should().Be(item.ProductId);
        result.Quantity.Should().Be(item.Quantity);
    }

    [Fact(DisplayName = "Given Cart with multiple items When mapping to CreateCartResult Then all items mapped")]
    public void Given_CartWithMultipleItems_When_MappingToCreateCartResult_Then_AllItemsMapped()
    {
        var cart = CartTestData.GenerateValidCart();
        cart.AddItem(CartTestData.GenerateValidCartItem(quantity: 5));

        var result = _mapper.Map<CreateCartResult>(cart);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
    }
}

public class ProductMappingProfileTests
{
    private readonly IMapper _mapper;

    public ProductMappingProfileTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CreateProductProfile>();
            cfg.AddProfile<GetProductProfile>();
            cfg.AddProfile<ProductListProfile>();
            cfg.AddProfile<UpdateProductProfile>();
        }, NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact(DisplayName = "Given CreateProductCommand When mapping to Product Then maps correctly")]
    public void Given_CreateProductCommand_When_MappingToProduct_Then_MapsCorrectly()
    {
        var command = new CreateProductCommand
        {
            Title = "Test Product", Price = 99.99m, Description = "Test Description",
            Category = "Test Category", Image = "https://example.com/image.jpg",
            Rating = new CreateProductRatingCommand { Rate = 4.5m, Count = 100 }
        };
        var result = _mapper.Map<Product>(command);
        result.Should().NotBeNull();
        result.Title.Should().Be(command.Title);
        result.Price.Should().Be(command.Price);
        result.Rating.Rate.Should().Be(command.Rating.Rate);
    }

    [Fact(DisplayName = "Given Product When mapping to CreateProductResult Then maps correctly")]
    public void Given_Product_When_MappingToCreateProductResult_Then_MapsCorrectly()
    {
        var product = ProductTestData.GenerateValidProduct();
        var result = _mapper.Map<CreateProductResult>(product);
        result.Should().NotBeNull();
        result.Title.Should().Be(product.Title);
        result.Price.Should().Be(product.Price);
        result.Rating.Rate.Should().Be(product.Rating.Rate);
    }

    [Fact(DisplayName = "Given Product When mapping to GetProductResult Then maps correctly")]
    public void Given_Product_When_MappingToGetProductResult_Then_MapsCorrectly()
    {
        var product = ProductTestData.GenerateValidProduct();
        var result = _mapper.Map<GetProductResult>(product);
        result.Should().NotBeNull();
        result.Title.Should().Be(product.Title);
        result.Category.Should().Be(product.Category);
    }

    [Fact(DisplayName = "Given Product When mapping to ProductListResult Then maps correctly")]
    public void Given_Product_When_MappingToProductListResult_Then_MapsCorrectly()
    {
        var product = ProductTestData.GenerateValidProduct();
        var result = _mapper.Map<ProductListResult>(product);
        result.Should().NotBeNull();
        result.Title.Should().Be(product.Title);
        result.Price.Should().Be(product.Price);
    }

    [Fact(DisplayName = "Given UpdateProductRatingCommand When mapping to ProductRating Then maps correctly")]
    public void Given_UpdateProductRatingCommand_When_MappingToProductRating_Then_MapsCorrectly()
    {
        var command = new UpdateProductRatingCommand { Rate = 4.2m, Count = 50 };
        var result = _mapper.Map<ProductRating>(command);
        result.Should().NotBeNull();
        result.Rate.Should().Be(command.Rate);
        result.Count.Should().Be(command.Count);
    }

    [Fact(DisplayName = "Given Product When mapping to UpdateProductResult Then maps correctly")]
    public void Given_Product_When_MappingToUpdateProductResult_Then_MapsCorrectly()
    {
        var product = ProductTestData.GenerateValidProduct();
        var result = _mapper.Map<UpdateProductResult>(product);
        result.Should().NotBeNull();
        result.Title.Should().Be(product.Title);
    }
}

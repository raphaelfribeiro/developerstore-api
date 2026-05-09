using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Application.Products.DeleteProduct;
using Ambev.DeveloperEvaluation.Application.Products.GetProduct;
using Ambev.DeveloperEvaluation.Application.Products.GetProducts;
using Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using Bogus;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Products;

public class CreateProductHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly CreateProductHandler _handler;
    private static readonly Faker Faker = new();

    public CreateProductHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CreateProductHandler(_productRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid product command When creating Then returns success response")]
    public async Task Handle_ValidCommand_ReturnsSuccessResponse()
    {
        var command = new CreateProductCommand
        {
            Title = Faker.Commerce.ProductName(),
            Price = 99.99m,
            Description = Faker.Commerce.ProductDescription(),
            Category = Faker.Commerce.Department(),
            Image = Faker.Internet.Url(),
            Rating = new CreateProductRatingCommand { Rate = 4.5m, Count = 100 }
        };

        var product = ProductTestData.GenerateValidProduct();
        _mapper.Map<Product>(command).Returns(product);
        _productRepository.CreateAsync(product, Arg.Any<CancellationToken>()).Returns(product);
        _mapper.Map<CreateProductResult>(product).Returns(new CreateProductResult());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        await _productRepository.Received(1).CreateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid product command When creating Then throws ValidationException")]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        var command = new CreateProductCommand();
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}

public class GetProductHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly GetProductHandler _handler;

    public GetProductHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetProductHandler(_productRepository, _mapper);
    }

    [Fact(DisplayName = "Given existing product ID When getting product Then returns product result")]
    public async Task Handle_ExistingProductId_ReturnsProductResult()
    {
        // Given — product.Id is now Guid.NewGuid()
        var product = ProductTestData.GenerateValidProduct();
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _mapper.Map<GetProductResult>(product).Returns(new GetProductResult { Id = product.Id });

        var result = await _handler.Handle(new GetProductQuery(product.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(product.Id);
    }

    [Fact(DisplayName = "Given non-existent product ID When getting product Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentProductId_ThrowsKeyNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        _productRepository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);
        var act = () => _handler.Handle(new GetProductQuery(nonExistentId), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{nonExistentId}*");
    }
}

public class DeleteProductHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly DeleteProductHandler _handler;

    public DeleteProductHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _handler = new DeleteProductHandler(_productRepository);
    }

    [Fact(DisplayName = "Given existing product When deleting Then returns success")]
    public async Task Handle_ExistingProduct_ReturnsSuccess()
    {
        var productId = Guid.NewGuid();
        _productRepository.DeleteAsync(productId, Arg.Any<CancellationToken>()).Returns(true);
        var result = await _handler.Handle(new DeleteProductCommand(productId), CancellationToken.None);
        result.Success.Should().BeTrue();
    }

    [Fact(DisplayName = "Given non-existent product When deleting Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentProduct_ThrowsKeyNotFoundException()
    {
        _productRepository.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var act = () => _handler.Handle(new DeleteProductCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}

public class UpdateProductHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly UpdateProductHandler _handler;

    public UpdateProductHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new UpdateProductHandler(_productRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid update command When updating product Then returns updated product")]
    public async Task Handle_ValidCommand_ReturnsUpdatedProduct()
    {
        var product = ProductTestData.GenerateValidProduct();
        var command = new UpdateProductCommand
        {
            Id = product.Id, // product.Id is now Guid.NewGuid()
            Title = "Updated Title",
            Price = 199.99m,
            Description = "Updated Description",
            Category = "Updated Category",
            Image = "https://example.com/image.jpg",
            Rating = new UpdateProductRatingCommand { Rate = 4.5m, Count = 100 }
        };

        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _productRepository.UpdateAsync(product, Arg.Any<CancellationToken>()).Returns(product);
        _mapper.Map<ProductRating>(command.Rating).Returns(new ProductRating { Rate = 4.5m, Count = 100 });
        _mapper.Map<UpdateProductResult>(product).Returns(new UpdateProductResult { Id = product.Id });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        await _productRepository.Received(1).UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existent product When updating Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentProduct_ThrowsKeyNotFoundException()
    {
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Price = 10m,
            Description = "Desc",
            Category = "Cat",
            Rating = new UpdateProductRatingCommand { Rate = 4m, Count = 10 }
        };
        _productRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Product?)null);
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given invalid update command When updating product Then throws ValidationException")]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        var command = new UpdateProductCommand();
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}

public class GetProductsHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductsHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _mapper = Substitute.For<IMapper>();
    }

    [Fact(DisplayName = "Given valid query When getting products Then returns paginated result")]
    public async Task GetProducts_ValidQuery_ReturnsPaginatedResult()
    {
        var handler = new GetProductsHandler(_productRepository, _mapper);
        var products = new List<Product>
        {
            ProductTestData.GenerateValidProduct(),
            ProductTestData.GenerateValidProduct()
        }.BuildMock();

        _productRepository.GetAllQueryable().Returns(products);
        _mapper.Map<List<ProductListResult>>(Arg.Any<object>())
            .Returns(new List<ProductListResult> { new(), new() });

        var result = await handler.Handle(
            new GetProductsQuery { Page = 1, Size = 10 }, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Given query with wildcard title When getting products Then filters with contains")]
    public async Task GetProducts_QueryWithWildcardTitle_FiltersWithContains()
    {
        var handler = new GetProductsHandler(_productRepository, _mapper);
        var p1 = ProductTestData.GenerateValidProduct(); p1.Title = "Fjallraven Backpack";
        var p2 = ProductTestData.GenerateValidProduct(); p2.Title = "Fjallraven Jacket";
        var p3 = ProductTestData.GenerateValidProduct(); p3.Title = "Other Product";

        var products = new List<Product> { p1, p2, p3 }.BuildMock();
        _productRepository.GetAllQueryable().Returns(products);
        _mapper.Map<List<ProductListResult>>(Arg.Any<object>())
            .Returns(new List<ProductListResult> { new(), new() });

        var result = await handler.Handle(
            new GetProductsQuery { Page = 1, Size = 10, Title = "Fjallraven*" }, CancellationToken.None);

        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Given query with price range When getting products Then filters correctly")]
    public async Task GetProducts_QueryWithPriceRange_FiltersCorrectly()
    {
        var handler = new GetProductsHandler(_productRepository, _mapper);
        var p1 = ProductTestData.GenerateValidProduct(); p1.Price = 50m;
        var p2 = ProductTestData.GenerateValidProduct(); p2.Price = 200m;
        var p3 = ProductTestData.GenerateValidProduct(); p3.Price = 500m;

        var products = new List<Product> { p1, p2, p3 }.BuildMock();
        _productRepository.GetAllQueryable().Returns(products);
        _mapper.Map<List<ProductListResult>>(Arg.Any<object>())
            .Returns(new List<ProductListResult> { new(), new() });

        var result = await handler.Handle(
            new GetProductsQuery { Page = 1, Size = 10, MinPrice = 40m, MaxPrice = 250m },
            CancellationToken.None);

        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Given categories exist When getting categories Then returns distinct categories")]
    public async Task GetCategories_CategoriesExist_ReturnsDistinctCategories()
    {
        var handler = new GetCategoriesHandler(_productRepository);
        var categories = new List<string> { "electronics", "clothing", "books" };
        _productRepository.GetCategoriesAsync(Arg.Any<CancellationToken>()).Returns(categories);

        var result = await handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().Contain("electronics");
    }

    [Fact(DisplayName = "Given valid category When getting products by category Then returns filtered results")]
    public async Task GetProductsByCategory_ValidCategory_ReturnsFilteredResults()
    {
        var handler = new GetProductsByCategoryHandler(_productRepository, _mapper);
        var p1 = ProductTestData.GenerateValidProduct(); p1.Category = "electronics";
        var p2 = ProductTestData.GenerateValidProduct(); p2.Category = "electronics";

        var products = new List<Product> { p1, p2 }.BuildMock();
        _productRepository.GetByCategoryQueryable("electronics").Returns(products);
        _mapper.Map<List<ProductListResult>>(Arg.Any<object>())
            .Returns(new List<ProductListResult> { new(), new() });

        var result = await handler.Handle(
            new GetProductsByCategoryQuery { Category = "electronics", Page = 1, Size = 10 },
            CancellationToken.None);

        result.TotalCount.Should().Be(2);
    }
}

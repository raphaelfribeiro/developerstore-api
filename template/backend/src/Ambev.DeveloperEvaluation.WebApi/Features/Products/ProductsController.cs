using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Application.Products.DeleteProduct;
using Ambev.DeveloperEvaluation.Application.Products.GetProduct;
using Ambev.DeveloperEvaluation.Application.Products.GetProducts;
using Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;
using Ambev.DeveloperEvaluation.WebApi.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products;

/// <summary>
/// Controller for managing product catalogue operations.
/// Provides full CRUD, category listing, and category-filtered product queries.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public ProductsController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new product in the catalogue.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateProductResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        var validator = new CreateProductRequestValidator();
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.First().ErrorMessage);

        var command = _mapper.Map<CreateProductCommand>(request);
        var result = await _mediator.Send(command, cancellationToken);
        var response = _mapper.Map<CreateProductResponse>(result);

        return base.Created($"/api/products/{response.Id}", new ApiResponseWithData<CreateProductResponse>
        {
            Success = true,
            Message = "Product created successfully",
            Data = response
        });
    }

    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductQuery(id), cancellationToken);
        return Ok(_mapper.Map<GetProductResponse>(result));
    }

    /// <summary>
    /// Retrieves all distinct product categories.
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponseWithData<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);

        return Ok(categories, "Categories retrieved successfully");
    }

    /// <summary>
    /// Retrieves a paginated list of products filtered by category.
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(PaginatedResponse<GetProductsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsByCategory(
        [FromRoute] string category,
        [FromQuery(Name = "_page")] int page = 1,
        [FromQuery(Name = "_size")] int size = 10,
        [FromQuery(Name = "_order")] string? order = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsByCategoryQuery { Category = category, Page = page, Size = size, Order = order };
        var result = await _mediator.Send(query, cancellationToken);
        var mapped = _mapper.Map<List<GetProductsResponse>>(result.Data);

        return OkPaginated(new Application.Common.PaginatedResult<GetProductsResponse>(
            mapped, result.TotalCount, result.CurrentPage, result.PageSize));
    }

    /// <summary>
    /// Retrieves a paginated, filterable and sortable list of products.
    /// Supports wildcard title search using * prefix/suffix (e.g. "fjallraven*").
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<GetProductsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery(Name = "_page")] int page = 1,
        [FromQuery(Name = "_size")] int size = 10,
        [FromQuery(Name = "_order")] string? order = null,
        [FromQuery] string? title = null,
        [FromQuery] string? category = null,
        [FromQuery(Name = "_minPrice")] decimal? minPrice = null,
        [FromQuery(Name = "_maxPrice")] decimal? maxPrice = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsQuery
        {
            Page = page,
            Size = size,
            Order = order,
            Title = title,
            Category = category,
            MinPrice = minPrice,
            MaxPrice = maxPrice
        };

        var result = await _mediator.Send(query, cancellationToken);
        var mapped = _mapper.Map<List<GetProductsResponse>>(result.Data);

        return OkPaginated(new Application.Common.PaginatedResult<GetProductsResponse>(
            mapped, result.TotalCount, result.CurrentPage, result.PageSize));
    }

    /// <summary>
    /// Updates an existing product's information.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        [FromRoute] Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest("Request body is required.");
            
        var validator = new UpdateProductRequestValidator();
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.First().ErrorMessage);

        var command = _mapper.Map<UpdateProductCommand>(request);
        command.Id = id;

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(_mapper.Map<UpdateProductResponse>(result), "Product updated successfully");
    }

    /// <summary>
    /// Deletes a product by its unique identifier.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);

        return Ok(new { }, "Product deleted successfully");
    }
}

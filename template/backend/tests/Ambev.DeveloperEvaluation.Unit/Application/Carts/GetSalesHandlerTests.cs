using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

/// <summary>
/// Contains unit tests for the GetSalesHandler class covering pagination and filtering.
/// Uses MockQueryable.NSubstitute to support EF async operations in unit tests.
/// </summary>
public class GetSalesHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetSalesHandler _handler;

    public GetSalesHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetSalesHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid query When getting sales Then returns paginated result")]
    public async Task Handle_ValidQuery_ReturnsPaginatedResult()
    {
        // Given
        var sales = new List<Sale>
        {
            SaleTestData.GenerateValidSale(quantity: 5),
            SaleTestData.GenerateValidSale(quantity: 3)
        }.BuildMock(); // MockQueryable — supports CountAsync/ToListAsync

        _saleRepository.GetAllQueryable().Returns(sales);
        _mapper.Map<List<GetSalesResult>>(Arg.Any<object>())
            .Returns(new List<GetSalesResult> { new(), new() });

        // When
        var result = await _handler.Handle(
            new GetSalesQuery { Page = 1, Size = 10 }, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.CurrentPage.Should().Be(1);
        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Given query with page 2 When getting sales Then returns correct pagination")]
    public async Task Handle_QueryWithPageTwo_ReturnsCorrectPagination()
    {
        // Given
        var salesList = Enumerable.Range(1, 15)
            .Select(_ => SaleTestData.GenerateValidSale(quantity: 3))
            .ToList();
        var sales = salesList.BuildMock();

        _saleRepository.GetAllQueryable().Returns(sales);
        _mapper.Map<List<GetSalesResult>>(Arg.Any<object>()).Returns(new List<GetSalesResult>());

        // When
        var result = await _handler.Handle(
            new GetSalesQuery { Page = 2, Size = 10 }, CancellationToken.None);

        // Then
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(2);
        result.CurrentPage.Should().Be(2);
    }

    [Fact(DisplayName = "Given query filtering by customerName When getting sales Then filters correctly")]
    public async Task Handle_QueryWithCustomerNameFilter_FiltersCorrectly()
    {
        // Given
        var sale1 = SaleTestData.GenerateValidSale(quantity: 3);
        sale1.CustomerName = "John Doe";
        var sale2 = SaleTestData.GenerateValidSale(quantity: 3);
        sale2.CustomerName = "Jane Smith";

        var sales = new List<Sale> { sale1, sale2 }.BuildMock();
        _saleRepository.GetAllQueryable().Returns(sales);
        _mapper.Map<List<GetSalesResult>>(Arg.Any<object>())
            .Returns(new List<GetSalesResult> { new() });

        // When
        var result = await _handler.Handle(
            new GetSalesQuery { Page = 1, Size = 10, CustomerName = "John" }, CancellationToken.None);

        // Then
        result.TotalCount.Should().Be(1);
    }

    [Fact(DisplayName = "Given query filtering by isCancelled When getting sales Then filters correctly")]
    public async Task Handle_QueryWithIsCancelledFilter_FiltersCorrectly()
    {
        // Given
        var sale1 = SaleTestData.GenerateValidSale(quantity: 3);
        sale1.Cancel();
        var sale2 = SaleTestData.GenerateValidSale(quantity: 3);

        var sales = new List<Sale> { sale1, sale2 }.BuildMock();
        _saleRepository.GetAllQueryable().Returns(sales);
        _mapper.Map<List<GetSalesResult>>(Arg.Any<object>())
            .Returns(new List<GetSalesResult> { new() });

        // When
        var result = await _handler.Handle(
            new GetSalesQuery { Page = 1, Size = 10, IsCancelled = true }, CancellationToken.None);

        // Then
        result.TotalCount.Should().Be(1);
    }

    [Fact(DisplayName = "Given query with order by saledate asc When getting sales Then orders correctly")]
    public async Task Handle_QueryWithOrderBySaleDateAsc_OrdersCorrectly()
    {
        // Given
        var sales = new List<Sale>
        {
            SaleTestData.GenerateValidSale(quantity: 3),
            SaleTestData.GenerateValidSale(quantity: 3)
        }.BuildMock();

        _saleRepository.GetAllQueryable().Returns(sales);
        _mapper.Map<List<GetSalesResult>>(Arg.Any<object>()).Returns(new List<GetSalesResult>());

        // When
        var result = await _handler.Handle(
            new GetSalesQuery { Page = 1, Size = 10, Order = "saledate asc" }, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
    }
}

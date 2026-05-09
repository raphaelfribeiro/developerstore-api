using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Mappings;

public class SaleMappingProfileTests
{
    private readonly IMapper _mapper;

    public SaleMappingProfileTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CreateSaleProfile>();
            cfg.AddProfile<GetSaleProfile>();
            cfg.AddProfile<GetSalesProfile>();
            cfg.AddProfile<UpdateSaleProfile>();
        }, NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact(DisplayName = "Given CreateSaleItemCommand When mapping to SaleItem Then maps correctly")]
    public void Given_CreateSaleItemCommand_When_Mapping_Then_MapsCorrectly()
    {
        var command = new CreateSaleItemCommand
        {
            ProductId = Guid.NewGuid(), ProductName = "Test Product", Quantity = 5, UnitPrice = 100m
        };
        var result = _mapper.Map<SaleItem>(command);
        result.Should().NotBeNull();
        result.ProductId.Should().Be(command.ProductId);
        result.Quantity.Should().Be(command.Quantity);
    }

    [Fact(DisplayName = "Given Sale When mapping to CreateSaleResult Then maps correctly")]
    public void Given_Sale_When_MappingToCreateSaleResult_Then_MapsCorrectly()
    {
        var sale = SaleTestData.GenerateValidSale(quantity: 5);
        var result = _mapper.Map<CreateSaleResult>(sale);
        result.Should().NotBeNull();
        result.SaleNumber.Should().Be(sale.SaleNumber);
        result.TotalAmount.Should().Be(sale.TotalAmount);
    }

    [Fact(DisplayName = "Given Sale When mapping to GetSaleResult Then maps correctly")]
    public void Given_Sale_When_MappingToGetSaleResult_Then_MapsCorrectly()
    {
        var sale = SaleTestData.GenerateValidSale(quantity: 5);
        var result = _mapper.Map<GetSaleResult>(sale);
        result.Should().NotBeNull();
        result.SaleNumber.Should().Be(sale.SaleNumber);
        result.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Given SaleItem When mapping to GetSaleItemResult Then maps correctly")]
    public void Given_SaleItem_When_MappingToGetSaleItemResult_Then_MapsCorrectly()
    {
        var item = SaleTestData.GenerateValidSaleItem(quantity: 5, unitPrice: 100m);
        var result = _mapper.Map<GetSaleItemResult>(item);
        result.Should().NotBeNull();
        result.ProductId.Should().Be(item.ProductId);
        result.Quantity.Should().Be(item.Quantity);
    }

    [Fact(DisplayName = "Given Sale When mapping to GetSalesResult Then maps correctly")]
    public void Given_Sale_When_MappingToGetSalesResult_Then_MapsCorrectly()
    {
        var sale = SaleTestData.GenerateValidSale(quantity: 5);
        var result = _mapper.Map<GetSalesResult>(sale);
        result.Should().NotBeNull();
        result.SaleNumber.Should().Be(sale.SaleNumber);
        result.ItemCount.Should().Be(1);
    }

    [Fact(DisplayName = "Given UpdateSaleItemCommand When mapping to SaleItem Then maps correctly")]
    public void Given_UpdateSaleItemCommand_When_Mapping_Then_MapsCorrectly()
    {
        var command = new UpdateSaleItemCommand
        {
            ProductId = Guid.NewGuid(), ProductName = "Updated Product", Quantity = 10, UnitPrice = 200m
        };
        var result = _mapper.Map<SaleItem>(command);
        result.Should().NotBeNull();
        result.ProductId.Should().Be(command.ProductId);
        result.Quantity.Should().Be(command.Quantity);
    }

    [Fact(DisplayName = "Given Sale When mapping to UpdateSaleResult Then maps correctly")]
    public void Given_Sale_When_MappingToUpdateSaleResult_Then_MapsCorrectly()
    {
        var sale = SaleTestData.GenerateValidSale(quantity: 5);
        var result = _mapper.Map<UpdateSaleResult>(sale);
        result.Should().NotBeNull();
        result.SaleNumber.Should().Be(sale.SaleNumber);
    }
}

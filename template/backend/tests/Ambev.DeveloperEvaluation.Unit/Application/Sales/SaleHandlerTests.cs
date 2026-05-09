using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

// ═══════════════════════════════════════════════════════════════════
// GET SALE
// ═══════════════════════════════════════════════════════════════════

public class GetSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetSaleHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given existing sale ID When getting sale Then returns sale result")]
    public async Task Handle_ExistingSaleId_ReturnsSaleResult()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(quantity: 5);
        // sale.Id is now Guid.NewGuid() — guaranteed non-empty
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(new GetSaleResult { Id = sale.Id });

        // When
        var result = await _handler.Handle(new GetSaleQuery(sale.Id), CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Id.Should().Be(sale.Id);
    }

    [Fact(DisplayName = "Given non-existent sale ID When getting sale Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentSaleId_ThrowsKeyNotFoundException()
    {
        // Given
        var nonExistentId = Guid.NewGuid();
        _saleRepository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(new GetSaleQuery(nonExistentId), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{nonExistentId}*");
    }

    [Fact(DisplayName = "Given empty sale ID When getting sale Then throws ValidationException")]
    public async Task Handle_EmptySaleId_ThrowsValidationException()
    {
        var act = () => _handler.Handle(new GetSaleQuery(Guid.Empty), CancellationToken.None);
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}

// ═══════════════════════════════════════════════════════════════════
// UPDATE SALE
// ═══════════════════════════════════════════════════════════════════

public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _eventPublisher = Substitute.For<IEventPublisher>();
        _mapper = Substitute.For<IMapper>();
        _handler = new UpdateSaleHandler(_saleRepository, _eventPublisher, _mapper);
    }

    [Fact(DisplayName = "Given valid update command When updating sale Then publishes SaleModifiedEvent")]
    public async Task Handle_ValidCommand_PublishesSaleModifiedEvent()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(quantity: 5);
        var command = new UpdateSaleCommand
        {
            Id = sale.Id, // sale.Id is now Guid.NewGuid()
            SaleNumber = "SALE-UPDATED",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Updated Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Updated Branch",
            Items = new List<UpdateSaleItemCommand>
            {
                new() { ProductId = Guid.NewGuid(), ProductName = "Product", Quantity = 5, UnitPrice = 100m }
            }
        };

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<List<SaleItem>>(Arg.Any<List<UpdateSaleItemCommand>>())
            .Returns(new List<SaleItem>
            {
                new SaleItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product",
                    Quantity = 5,
                    UnitPrice = 100m
                }
            });
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<UpdateSaleResult>(Arg.Any<Sale>()).Returns(new UpdateSaleResult());

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Any<SaleModifiedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existent sale When updating Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentSale_ThrowsKeyNotFoundException()
    {
        // Given
        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            Items = new List<UpdateSaleItemCommand>
            {
                new() { ProductId = Guid.NewGuid(), ProductName = "Product", Quantity = 1, UnitPrice = 10m }
            }
        };
        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}

// ═══════════════════════════════════════════════════════════════════
// DELETE SALE
// ═══════════════════════════════════════════════════════════════════

public class DeleteSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly DeleteSaleHandler _handler;

    public DeleteSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _eventPublisher = Substitute.For<IEventPublisher>();
        _handler = new DeleteSaleHandler(_saleRepository, _eventPublisher);
    }

    [Fact(DisplayName = "Given existing sale When deleting Then publishes SaleCancelledEvent")]
    public async Task Handle_ExistingSale_PublishesSaleCancelledEvent()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(quantity: 5);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.SaveAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.DeleteAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(true);

        // When
        var result = await _handler.Handle(new DeleteSaleCommand(sale.Id), CancellationToken.None);

        // Then
        result.Success.Should().BeTrue();
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Any<SaleCancelledEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existent sale When deleting Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentSale_ThrowsKeyNotFoundException()
    {
        // Given
        var nonExistentId = Guid.NewGuid();
        _saleRepository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(new DeleteSaleCommand(nonExistentId), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}

// ═══════════════════════════════════════════════════════════════════
// CANCEL SALE ITEM
// ═══════════════════════════════════════════════════════════════════

public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _eventPublisher = Substitute.For<IEventPublisher>();
        _handler = new CancelSaleItemHandler(_saleRepository, _eventPublisher);
    }

    [Fact(DisplayName = "Given existing sale item When cancelling Then publishes ItemCancelledEvent")]
    public async Task Handle_ExistingItem_PublishesItemCancelledEvent()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(quantity: 5);
        var itemId = sale.Items.First().Id; // Id is now Guid.NewGuid()

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.SaveAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var result = await _handler.Handle(
            new CancelSaleItemCommand(sale.Id, itemId), CancellationToken.None);

        // Then
        result.Success.Should().BeTrue();
        result.ItemId.Should().Be(itemId);
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Any<ItemCancelledEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existent sale When cancelling item Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentSale_ThrowsKeyNotFoundException()
    {
        // Given
        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(
            new CancelSaleItemCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given non-existent item When cancelling Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentItem_ThrowsKeyNotFoundException()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(quantity: 5);
        var nonExistentItemId = Guid.NewGuid();
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var act = () => _handler.Handle(
            new CancelSaleItemCommand(sale.Id, nonExistentItemId), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}

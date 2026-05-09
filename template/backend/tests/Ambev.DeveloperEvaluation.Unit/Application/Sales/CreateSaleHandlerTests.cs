using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

/// <summary>
/// Contains unit tests for the CreateSaleHandler class.
/// </summary>
public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _eventPublisher = Substitute.For<IEventPublisher>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CreateSaleHandler(_saleRepository, _eventPublisher, _mapper);
    }

    [Fact(DisplayName = "Given valid sale command When creating sale Then returns success response")]
    public async Task Handle_ValidCommand_ReturnsSuccessResponse()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        SetupMapperForCommand(command);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given valid sale When creating Then publishes SaleCreatedEvent")]
    public async Task Handle_ValidCommand_PublishesSaleCreatedEvent()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        SetupMapperForCommand(command);

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Any<SaleCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given item with 5 units When creating sale Then applies 10% discount")]
    public async Task Handle_ItemWithFiveUnits_AppliesTenPercentDiscount()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateCommandWithTenPercentDiscount();
        var capturedSale = (Sale?)null;

        // Setup mapper to return real SaleItem (not null)
        _mapper.Map<SaleItem>(Arg.Any<CreateSaleItemCommand>())
            .Returns(ci =>
            {
                var cmd = ci.Arg<CreateSaleItemCommand>();
                return new SaleItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = cmd.ProductId,
                    ProductName = cmd.ProductName,
                    Quantity = cmd.Quantity,
                    UnitPrice = cmd.UnitPrice
                };
            });

        _saleRepository.CreateAsync(Arg.Do<Sale>(s => capturedSale = s), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult());

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        capturedSale.Should().NotBeNull();
        capturedSale!.Items.First().Discount.Should().Be(0.10m);
    }

    [Fact(DisplayName = "Given item with 10 units When creating sale Then applies 20% discount")]
    public async Task Handle_ItemWithTenUnits_AppliesTwentyPercentDiscount()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateCommandWithTwentyPercentDiscount();
        var capturedSale = (Sale?)null;

        _mapper.Map<SaleItem>(Arg.Any<CreateSaleItemCommand>())
            .Returns(ci =>
            {
                var cmd = ci.Arg<CreateSaleItemCommand>();
                return new SaleItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = cmd.ProductId,
                    ProductName = cmd.ProductName,
                    Quantity = cmd.Quantity,
                    UnitPrice = cmd.UnitPrice
                };
            });

        _saleRepository.CreateAsync(Arg.Do<Sale>(s => capturedSale = s), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult());

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        capturedSale.Should().NotBeNull();
        capturedSale!.Items.First().Discount.Should().Be(0.20m);
    }

    [Fact(DisplayName = "Given item with more than 20 units When creating sale Then throws ValidationException")]
    public async Task Handle_ItemExceedingMaxQuantity_ThrowsValidationException()
    {
        // Given — the validator runs before the domain, so ValidationException is thrown first
        var command = CreateSaleHandlerTestData.GenerateCommandWithExceedingQuantity();

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given invalid command When creating sale Then throws ValidationException")]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        // Given
        var command = new CreateSaleCommand();

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    private void SetupMapperForCommand(CreateSaleCommand command)
    {
        _mapper.Map<SaleItem>(Arg.Any<CreateSaleItemCommand>())
            .Returns(ci =>
            {
                var cmd = ci.Arg<CreateSaleItemCommand>();
                return new SaleItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = cmd.ProductId,
                    ProductName = cmd.ProductName,
                    Quantity = cmd.Quantity,
                    UnitPrice = cmd.UnitPrice
                };
            });

        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult());
    }
}

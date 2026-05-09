using Ambev.DeveloperEvaluation.Application.Carts.CreateCart;
using Ambev.DeveloperEvaluation.Application.Carts.DeleteCart;
using Ambev.DeveloperEvaluation.Application.Carts.GetCart;
using Ambev.DeveloperEvaluation.Application.Carts.GetCarts;
using Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Carts;

public class CreateCartHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;
    private readonly CreateCartHandler _handler;

    public CreateCartHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CreateCartHandler(_cartRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid cart command When creating cart Then returns success response")]
    public async Task Handle_ValidCommand_ReturnsSuccessResponse()
    {
        var command = new CreateCartCommand
        {
            UserId = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            Items = new List<CreateCartItemCommand>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 3 }
            }
        };
        _cartRepository.CreateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Cart>());
        _mapper.Map<CreateCartResult>(Arg.Any<Cart>()).Returns(new CreateCartResult());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        await _cartRepository.Received(1).CreateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid cart command When creating Then throws ValidationException")]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        var command = new CreateCartCommand();
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}

public class GetCartHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;
    private readonly GetCartHandler _handler;

    public GetCartHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetCartHandler(_cartRepository, _mapper);
    }

    [Fact(DisplayName = "Given existing cart ID When getting cart Then returns cart result")]
    public async Task Handle_ExistingCartId_ReturnsCartResult()
    {
        // Given — cart.Id is now Guid.NewGuid()
        var cart = CartTestData.GenerateValidCart();
        _cartRepository.GetByIdAsync(cart.Id, Arg.Any<CancellationToken>()).Returns(cart);
        _mapper.Map<GetCartResult>(cart).Returns(new GetCartResult { Id = cart.Id });

        var result = await _handler.Handle(new GetCartQuery(cart.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(cart.Id);
    }

    [Fact(DisplayName = "Given non-existent cart ID When getting cart Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentCartId_ThrowsKeyNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        _cartRepository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>()).Returns((Cart?)null);
        var act = () => _handler.Handle(new GetCartQuery(nonExistentId), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{nonExistentId}*");
    }
}

public class UpdateCartHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;
    private readonly UpdateCartHandler _handler;

    public UpdateCartHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new UpdateCartHandler(_cartRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid update command When updating cart Then returns updated cart")]
    public async Task Handle_ValidCommand_ReturnsUpdatedCart()
    {
        var command = new UpdateCartCommand
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            Items = new List<UpdateCartItemCommand>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 5 }
            }
        };

        _cartRepository.UpdateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Cart>());
        _mapper.Map<UpdateCartResult>(Arg.Any<Cart>())
            .Returns(new UpdateCartResult { Id = command.Id });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        await _cartRepository.Received(1).UpdateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid update command When updating cart Then throws ValidationException")]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        var command = new UpdateCartCommand();
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}

public class DeleteCartHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly DeleteCartHandler _handler;

    public DeleteCartHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _handler = new DeleteCartHandler(_cartRepository);
    }

    [Fact(DisplayName = "Given existing cart When deleting Then returns success")]
    public async Task Handle_ExistingCart_ReturnsSuccess()
    {
        var cartId = Guid.NewGuid();
        _cartRepository.DeleteAsync(cartId, Arg.Any<CancellationToken>()).Returns(true);
        var result = await _handler.Handle(new DeleteCartCommand(cartId), CancellationToken.None);
        result.Success.Should().BeTrue();
    }

    [Fact(DisplayName = "Given non-existent cart When deleting Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentCart_ThrowsKeyNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        _cartRepository.DeleteAsync(nonExistentId, Arg.Any<CancellationToken>()).Returns(false);
        var act = () => _handler.Handle(new DeleteCartCommand(nonExistentId), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}

public class GetCartsHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;
    private readonly GetCartsHandler _handler;

    public GetCartsHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetCartsHandler(_cartRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid query When getting carts Then returns paginated result")]
    public async Task Handle_ValidQuery_ReturnsPaginatedResult()
    {
        var carts = new List<Cart>
        {
            CartTestData.GenerateValidCart(),
            CartTestData.GenerateValidCart()
        }.BuildMock();

        _cartRepository.GetAllQueryable().Returns(carts);
        _mapper.Map<List<GetCartsResult>>(Arg.Any<object>())
            .Returns(new List<GetCartsResult> { new(), new() });

        var result = await _handler.Handle(
            new GetCartsQuery { Page = 1, Size = 10 }, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Given query filtering by userId When getting carts Then filters correctly")]
    public async Task Handle_QueryWithUserIdFilter_FiltersCorrectly()
    {
        var userId = Guid.NewGuid();
        var cart1 = CartTestData.GenerateValidCart();
        cart1.UserId = userId;
        var cart2 = CartTestData.GenerateValidCart();

        var carts = new List<Cart> { cart1, cart2 }.BuildMock();
        _cartRepository.GetAllQueryable().Returns(carts);
        _mapper.Map<List<GetCartsResult>>(Arg.Any<object>())
            .Returns(new List<GetCartsResult> { new() });

        var result = await _handler.Handle(
            new GetCartsQuery { Page = 1, Size = 10, UserId = userId }, CancellationToken.None);

        result.TotalCount.Should().Be(1);
    }
}

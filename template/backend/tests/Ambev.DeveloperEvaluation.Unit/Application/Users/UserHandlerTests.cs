using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Users;

/// <summary>
/// Contains unit tests for the GetUserHandler class.
/// </summary>
public class GetUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly GetUserHandler _handler;

    public GetUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetUserHandler(_userRepository, _mapper);
    }

    [Fact(DisplayName = "Given existing user ID When getting user Then returns user result")]
    public async Task Handle_ExistingUserId_ReturnsUserResult()
    {
        // Given
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<GetUserResult>(user).Returns(new GetUserResult
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        });

        // When
        var result = await _handler.Handle(new GetUserCommand(user.Id), CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
    }

    [Fact(DisplayName = "Given non-existent user ID When getting user Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentUserId_ThrowsKeyNotFoundException()
    {
        // Given
        var nonExistentId = Guid.NewGuid();
        _userRepository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // When
        var act = () => _handler.Handle(new GetUserCommand(nonExistentId), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{nonExistentId}*");
    }

    [Fact(DisplayName = "Given empty user ID When getting user Then throws ValidationException")]
    public async Task Handle_EmptyUserId_ThrowsValidationException()
    {
        // Given
        var act = () => _handler.Handle(new GetUserCommand(Guid.Empty), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}

/// <summary>
/// Contains unit tests for the DeleteUserHandler class.
/// </summary>
public class DeleteUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteUserHandler _handler;

    public DeleteUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(1);
        _handler = new DeleteUserHandler(_userRepository, _unitOfWork);
    }

    [Fact(DisplayName = "Given existing user When deleting Then returns success")]
    public async Task Handle_ExistingUser_ReturnsSuccess()
    {
        // Given
        var userId = Guid.NewGuid();
        _userRepository.DeleteAsync(userId, Arg.Any<CancellationToken>()).Returns(true);

        // When
        var result = await _handler.Handle(
            new DeleteUserCommand(userId), CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact(DisplayName = "Given non-existent user When deleting Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentUser_ThrowsKeyNotFoundException()
    {
        // Given
        var nonExistentId = Guid.NewGuid();
        _userRepository.DeleteAsync(nonExistentId, Arg.Any<CancellationToken>()).Returns(false);

        // When
        var act = () => _handler.Handle(
            new DeleteUserCommand(nonExistentId), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given empty user ID When deleting Then throws ValidationException")]
    public async Task Handle_EmptyUserId_ThrowsValidationException()
    {
        // Given
        var act = () => _handler.Handle(
            new DeleteUserCommand(Guid.Empty), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}

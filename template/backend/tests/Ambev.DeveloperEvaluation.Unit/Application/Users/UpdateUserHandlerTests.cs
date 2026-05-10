using Ambev.DeveloperEvaluation.Application.Users.UpdateUser;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Users;

public class UpdateUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly UpdateUserHandler _handler;

    public UpdateUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _handler = new UpdateUserHandler(_userRepository, _mapper, _passwordHasher);
    }

    [Fact(DisplayName = "Given existing user When updating Then returns updated result")]
    public async Task Handle_ExistingUser_ReturnsUpdatedResult()
    {
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();

        var command = new UpdateUserCommand
        {
            Id = user.Id,
            Username = "updateduser",
            Email = "updated@test.com",
            Phone = "+5547999999999",
            FirstName = "Updated",
            LastName = "User",
            City = "Sao Paulo",
            Street = "Avenida Paulista",
            Number = 100,
            ZipCode = "01310-100",
            GeoLat = "-23.5613",
            GeoLong = "-46.6563"
        };

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _userRepository.UpdateAsync(user, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<UpdateUserResult>(user).Returns(new UpdateUserResult { Id = user.Id, Username = command.Username });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existent user When updating Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentUser_ThrowsKeyNotFoundException()
    {
        var command = new UpdateUserCommand { Id = Guid.NewGuid(), Username = "x", Email = "x@x.com" };
        _userRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{command.Id}*");
    }

    [Fact(DisplayName = "Given new non-empty password When updating Then password is hashed")]
    public async Task Handle_NewPassword_HashesPassword()
    {
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();

        var command = new UpdateUserCommand
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Password = "NewPassword@123"
        };

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _userRepository.UpdateAsync(user, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.HashPassword("NewPassword@123").Returns("hashed-pw");
        _mapper.Map<UpdateUserResult>(user).Returns(new UpdateUserResult());

        await _handler.Handle(command, CancellationToken.None);

        _passwordHasher.Received(1).HashPassword("NewPassword@123");
        user.Password.Should().Be("hashed-pw");
    }

    [Fact(DisplayName = "Given empty password When updating Then password is not re-hashed")]
    public async Task Handle_EmptyPassword_DoesNotHashPassword()
    {
        var user = UserTestData.GenerateValidUser();
        var originalPassword = user.Password;
        user.Id = Guid.NewGuid();

        var command = new UpdateUserCommand
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Password = ""
        };

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _userRepository.UpdateAsync(user, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<UpdateUserResult>(user).Returns(new UpdateUserResult());

        await _handler.Handle(command, CancellationToken.None);

        _passwordHasher.DidNotReceive().HashPassword(Arg.Any<string>());
        user.Password.Should().Be(originalPassword);
    }
}

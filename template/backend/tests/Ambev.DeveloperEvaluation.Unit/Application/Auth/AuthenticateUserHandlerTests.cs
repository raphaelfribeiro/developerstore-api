using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Auth;

public class AuthenticateUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AuthenticateUserHandler _handler;

    public AuthenticateUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _handler = new AuthenticateUserHandler(_userRepository, _passwordHasher, _jwtTokenGenerator);
    }

    [Fact(DisplayName = "Given valid credentials When authenticating Then returns token")]
    public async Task Handle_ValidCredentials_ReturnsToken()
    {
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Active;
        var command = new AuthenticateUserCommand { Email = user.Email, Password = "ValidPassword@123" };
        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(command.Password, user.Password).Returns(true);
        _jwtTokenGenerator.GenerateToken(user).Returns("jwt-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Token.Should().Be("jwt-token");
    }

    [Fact(DisplayName = "Given non-existent email When authenticating Then throws UnauthorizedAccessException")]
    public async Task Handle_NonExistentEmail_ThrowsUnauthorizedAccessException()
    {
        var command = new AuthenticateUserCommand { Email = "nonexistent@email.com", Password = "ValidPassword@123" };
        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns((User?)null);
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given wrong password When authenticating Then throws UnauthorizedAccessException")]
    public async Task Handle_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        var user = UserTestData.GenerateValidUser();
        var command = new AuthenticateUserCommand { Email = user.Email, Password = "WrongPassword" };
        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(command.Password, user.Password).Returns(false);
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given inactive user When authenticating Then throws UnauthorizedAccessException")]
    public async Task Handle_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        var user = UserTestData.GenerateValidUser();
        user.Status = UserStatus.Suspended;
        var command = new AuthenticateUserCommand { Email = user.Email, Password = "ValidPassword@123" };
        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(command.Password, user.Password).Returns(true);
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Given invalid command When authenticating Then throws UnauthorizedAccessException")]
    public async Task Handle_InvalidCommand_ThrowsUnauthorizedAccessException()
    {
        var command = new AuthenticateUserCommand();
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}

public class AuthenticateUserValidatorTests
{
    private readonly AuthenticateUserValidator _validator = new();

    [Fact(DisplayName = "Given valid credentials When validating Then returns valid")]
    public void Given_ValidCredentials_When_Validating_Then_ReturnsValid()
    {
        var command = new AuthenticateUserCommand { Email = "test@example.com", Password = "ValidPassword@123" };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Given empty email When validating Then returns invalid")]
    public void Given_EmptyEmail_When_Validating_Then_ReturnsInvalid()
    {
        var command = new AuthenticateUserCommand { Email = "", Password = "ValidPassword@123" };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact(DisplayName = "Given empty password When validating Then returns invalid")]
    public void Given_EmptyPassword_When_Validating_Then_ReturnsInvalid()
    {
        var command = new AuthenticateUserCommand { Email = "test@example.com", Password = "" };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}

public class AuthProfileTests
{
    [Fact(DisplayName = "Given AuthenticateUserProfile When mapping User to AuthenticateUserResult Then maps correctly")]
    public void Given_AuthenticateUserProfile_When_Mapping_Then_MapsCorrectly()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AuthenticateUserProfile>();
        }, NullLoggerFactory.Instance);
        var mapper = config.CreateMapper();
        var user = UserTestData.GenerateValidUser();

        var result = mapper.Map<AuthenticateUserResult>(user);

        result.Should().NotBeNull();
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be(user.Role.ToString());
    }
}

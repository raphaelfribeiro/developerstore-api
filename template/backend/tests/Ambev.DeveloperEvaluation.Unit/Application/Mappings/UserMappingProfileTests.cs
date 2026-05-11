using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Mappings;

public class UserMappingProfileTests
{
    [Fact(DisplayName = "Given User When mapping to CreateUserResult Then maps correctly")]
    public void Given_User_When_MappingToCreateUserResult_Then_MapsCorrectly()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CreateUserProfile>(); 
        }, NullLoggerFactory.Instance);
        var mapper = config.CreateMapper();
        var user = UserTestData.GenerateValidUser();

        var result = mapper.Map<CreateUserResult>(user);

        result.Should().NotBeNull();
        result.Email.Should().Be(user.Email);
        result.Username.Should().Be(user.Username);
        result.Role.Should().Be(user.Role);
        result.Status.Should().Be(user.Status);
    }

    [Fact(DisplayName = "Given User When mapping to GetUserResult Then maps correctly")]
    public void Given_User_When_MappingToGetUserResult_Then_MapsCorrectly()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<GetUserProfile>();
        }, NullLoggerFactory.Instance);
        var mapper = config.CreateMapper();
        var user = UserTestData.GenerateValidUser();

        var result = mapper.Map<GetUserResult>(user);

        result.Should().NotBeNull();
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be(user.Role);
        result.Status.Should().Be(user.Status);
    }
}

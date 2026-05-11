using Ambev.DeveloperEvaluation.Application.Users.GetUsers;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Users;

public class GetUsersHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly GetUsersHandler _handler;

    public GetUsersHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetUsersHandler(_userRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid query When getting users Then returns paginated result")]
    public async Task Handle_ValidQuery_ReturnsPaginatedResult()
    {
        var users = new[]
        {
            UserTestData.GenerateValidUser(),
            UserTestData.GenerateValidUser(),
            UserTestData.GenerateValidUser()
        }.AsQueryable().BuildMock();

        _userRepository.GetAllQueryable().Returns(users);
        _mapper.Map<List<GetUsersResult>>(Arg.Any<object>())
            .Returns(new List<GetUsersResult> { new(), new(), new() });

        var result = await _handler.Handle(
            new GetUsersQuery { Page = 1, Size = 10 }, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        result.CurrentPage.Should().Be(1);
        result.Data.Should().HaveCount(3);
    }

    [Fact(DisplayName = "Given query with page 2 When getting users Then returns correct page slice")]
    public async Task Handle_PageTwo_ReturnsCorrectSlice()
    {
        var users = Enumerable.Range(1, 15)
            .Select(_ => UserTestData.GenerateValidUser())
            .ToList()
            .AsQueryable().BuildMock();

        _userRepository.GetAllQueryable().Returns(users);
        _mapper.Map<List<GetUsersResult>>(Arg.Any<object>())
            .Returns(new List<GetUsersResult>(Enumerable.Range(1, 5).Select(_ => new GetUsersResult())));

        var result = await _handler.Handle(
            new GetUsersQuery { Page = 2, Size = 5 }, CancellationToken.None);

        result.TotalCount.Should().Be(15);
        result.CurrentPage.Should().Be(2);
    }

    [Fact(DisplayName = "Given empty user store When getting users Then returns zero total")]
    public async Task Handle_EmptyStore_ReturnsZeroTotal()
    {
        var users = new List<User>().AsQueryable().BuildMock();
        _userRepository.GetAllQueryable().Returns(users);
        _mapper.Map<List<GetUsersResult>>(Arg.Any<object>())
            .Returns(new List<GetUsersResult>());

        var result = await _handler.Handle(
            new GetUsersQuery { Page = 1, Size = 10 }, CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    [Fact(DisplayName = "Given order by email desc When getting users Then passes ordering")]
    public async Task Handle_OrderByEmailDesc_AppliesOrdering()
    {
        var users = new[]
        {
            UserTestData.GenerateValidUser(),
            UserTestData.GenerateValidUser()
        }.AsQueryable().BuildMock();

        _userRepository.GetAllQueryable().Returns(users);
        _mapper.Map<List<GetUsersResult>>(Arg.Any<object>())
            .Returns(new List<GetUsersResult> { new(), new() });

        var result = await _handler.Handle(
            new GetUsersQuery { Page = 1, Size = 10, Order = "email desc" }, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Given order by email asc When getting users Then orders ascending by email")]
    public async Task Handle_OrderByEmailAsc_AppliesAscendingEmailOrdering()
    {
        var users = new[]
        {
            UserTestData.GenerateValidUser(),
            UserTestData.GenerateValidUser()
        }.AsQueryable().BuildMock();

        _userRepository.GetAllQueryable().Returns(users);
        _mapper.Map<List<GetUsersResult>>(Arg.Any<object>())
            .Returns(new List<GetUsersResult> { new(), new() });

        var result = await _handler.Handle(
            new GetUsersQuery { Page = 1, Size = 10, Order = "email asc" }, CancellationToken.None);

        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Given order by username asc When getting users Then orders ascending by username")]
    public async Task Handle_OrderByUsernameAsc_AppliesAscendingUsernameOrdering()
    {
        var users = new[]
        {
            UserTestData.GenerateValidUser(),
            UserTestData.GenerateValidUser()
        }.AsQueryable().BuildMock();

        _userRepository.GetAllQueryable().Returns(users);
        _mapper.Map<List<GetUsersResult>>(Arg.Any<object>())
            .Returns(new List<GetUsersResult> { new(), new() });

        var result = await _handler.Handle(
            new GetUsersQuery { Page = 1, Size = 10, Order = "username asc" }, CancellationToken.None);

        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Given order by username desc When getting users Then orders descending by username")]
    public async Task Handle_OrderByUsernameDesc_AppliesDescendingUsernameOrdering()
    {
        var users = new[]
        {
            UserTestData.GenerateValidUser(),
            UserTestData.GenerateValidUser()
        }.AsQueryable().BuildMock();

        _userRepository.GetAllQueryable().Returns(users);
        _mapper.Map<List<GetUsersResult>>(Arg.Any<object>())
            .Returns(new List<GetUsersResult> { new(), new() });

        var result = await _handler.Handle(
            new GetUsersQuery { Page = 1, Size = 10, Order = "username desc" }, CancellationToken.None);

        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Given unrecognized order field When getting users Then falls back to default ordering")]
    public async Task Handle_UnrecognizedOrderField_FallsBackToDefaultOrdering()
    {
        var users = new[]
        {
            UserTestData.GenerateValidUser(),
            UserTestData.GenerateValidUser()
        }.AsQueryable().BuildMock();

        _userRepository.GetAllQueryable().Returns(users);
        _mapper.Map<List<GetUsersResult>>(Arg.Any<object>())
            .Returns(new List<GetUsersResult> { new(), new() });

        var result = await _handler.Handle(
            new GetUsersQuery { Page = 1, Size = 10, Order = "unknown field" }, CancellationToken.None);

        result.TotalCount.Should().Be(2);
    }
}

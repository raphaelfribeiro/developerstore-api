using Ambev.DeveloperEvaluation.Application.Users.PatchUserRole;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Users;

public class PatchUserRoleHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly PatchUserRoleHandler _handler;

    public PatchUserRoleHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(1);
        _mapper = Substitute.For<IMapper>();
        _handler = new PatchUserRoleHandler(_userRepository, _unitOfWork, _mapper);
    }

    [Fact(DisplayName = "Given existing user When patching role Then returns updated result")]
    public async Task Handle_ExistingUser_ReturnsUpdatedResult()
    {
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();

        var command = new PatchUserRoleCommand
        {
            Id = user.Id,
            Role = UserRole.Admin,
            Status = UserStatus.Active
        };

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _userRepository.UpdateAsync(user, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<PatchUserRoleResult>(user).Returns(new PatchUserRoleResult
        {
            Id = user.Id,
            Role = UserRole.Admin,
            Status = UserStatus.Active
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Role.Should().Be(UserRole.Admin);
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existent user When patching role Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentUser_ThrowsKeyNotFoundException()
    {
        var command = new PatchUserRoleCommand { Id = Guid.NewGuid(), Role = UserRole.Admin };
        _userRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{command.Id}*");
    }

    [Fact(DisplayName = "Given role change When patching Then role and status are persisted on user entity")]
    public async Task Handle_RoleChange_UpdatesUserRoleAndStatus()
    {
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();
        user.Role = UserRole.Customer;
        user.Status = UserStatus.Inactive;

        var command = new PatchUserRoleCommand
        {
            Id = user.Id,
            Role = UserRole.Manager,
            Status = UserStatus.Active
        };

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _userRepository.UpdateAsync(user, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<PatchUserRoleResult>(user).Returns(new PatchUserRoleResult());

        await _handler.Handle(command, CancellationToken.None);

        user.Role.Should().Be(UserRole.Manager);
        user.Status.Should().Be(UserStatus.Active);
    }
}

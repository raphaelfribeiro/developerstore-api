using Ambev.DeveloperEvaluation.Domain.Enums;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.PatchUserRole;

public class PatchUserRoleCommand : IRequest<PatchUserRoleResult>
{
    public Guid Id { get; set; }
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
}

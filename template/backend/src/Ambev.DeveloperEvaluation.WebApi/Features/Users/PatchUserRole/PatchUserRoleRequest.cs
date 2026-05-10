using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.PatchUserRole;

public class PatchUserRoleRequest
{
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
}

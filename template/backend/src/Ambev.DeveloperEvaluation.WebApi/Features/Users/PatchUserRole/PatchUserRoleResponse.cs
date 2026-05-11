namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.PatchUserRole;

public class PatchUserRoleResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

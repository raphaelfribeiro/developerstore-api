using Ambev.DeveloperEvaluation.WebApi.Features.Users.Shared;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.UpdateUser;

public class UpdateUserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserNameDto Name { get; set; } = new();
    public UserAddressDto Address { get; set; } = new();
    public string Phone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.Shared;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserNameDto Name { get; set; } = new();
    public UserAddressDto Address { get; set; } = new();
    public string Phone { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public UserRole Role { get; set; }
}

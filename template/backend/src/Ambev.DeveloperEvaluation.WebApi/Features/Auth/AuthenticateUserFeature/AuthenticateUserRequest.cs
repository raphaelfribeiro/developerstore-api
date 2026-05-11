namespace Ambev.DeveloperEvaluation.WebApi.Features.Auth.AuthenticateUserFeature;

/// <summary>
/// Represents the authentication request model for user login.
/// </summary>
public class AuthenticateUserRequest
{
    /// <summary>
    /// Gets or sets the username for authentication.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's password for authentication.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

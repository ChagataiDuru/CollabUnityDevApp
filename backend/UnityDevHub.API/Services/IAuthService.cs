using UnityDevHub.API.Models.Auth;

namespace UnityDevHub.API.Services;

/// <summary>
/// Interface for authentication services.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="dto">The registration data.</param>
    /// <returns>A token DTO containing the access and refresh tokens.</returns>
    Task<TokenDto> RegisterAsync(RegisterDto dto);

    /// <summary>
    /// Authenticates a user and returns tokens.
    /// </summary>
    /// <param name="dto">The login credentials.</param>
    /// <returns>A token DTO containing the access and refresh tokens.</returns>
    Task<TokenDto> LoginAsync(LoginDto dto);

    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    /// <param name="accessToken">The expired access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns>A new token DTO containing the access and refresh tokens.</returns>
    Task<TokenDto> RefreshTokenAsync(string accessToken, string refreshToken);

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RevokeRefreshTokenAsync(string refreshToken);
}

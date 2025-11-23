using Microsoft.AspNetCore.Mvc;
using UnityDevHub.API.Models.Auth;
using UnityDevHub.API.Services;

namespace UnityDevHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Controller for user authentication and authorization.
/// </summary>
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="dto">The registration data.</param>
    /// <returns>The authentication tokens if successful.</returns>
    [HttpPost("register")]
    public async Task<ActionResult<TokenDto>> Register(RegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Logs in an existing user.
    /// </summary>
    /// <param name="dto">The login credentials.</param>
    /// <returns>The authentication tokens if successful.</returns>
    [HttpPost("login")]
    public async Task<ActionResult<TokenDto>> Login(LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Refreshes the access token using a refresh token.
    /// </summary>
    /// <param name="tokenDto">The token data containing the expired access token and refresh token.</param>
    /// <returns>New authentication tokens if successful.</returns>
    [HttpPost("refresh")]
    public async Task<ActionResult<TokenDto>> Refresh([FromBody] TokenDto tokenDto)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(tokenDto.AccessToken, tokenDto.RefreshToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Logs out a user by revoking the refresh token.
    /// </summary>
    /// <param name="tokenDto">The token data containing the refresh token to revoke.</param>
    /// <returns>No content if successful.</returns>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] TokenDto tokenDto)
    {
        await _authService.RevokeRefreshTokenAsync(tokenDto.RefreshToken);
        return NoContent();
    }
}

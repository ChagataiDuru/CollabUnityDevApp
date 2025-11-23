using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace UnityDevHub.API.Controllers;

/// <summary>
/// Base controller providing common functionality for other controllers.
/// </summary>
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the current user's ID from the claims.
    /// </summary>
    protected Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
}

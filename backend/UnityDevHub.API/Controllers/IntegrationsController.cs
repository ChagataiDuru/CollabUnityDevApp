using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnityDevHub.API.Models.DevOps;
using UnityDevHub.API.Services;

namespace UnityDevHub.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/projects/{projectId}/integrations")]
    /// <summary>
    /// Controller for managing project integrations, such as version control systems.
    /// </summary>
    public class IntegrationsController : ControllerBase
    {
        private readonly IVcsService _vcsService;

        public IntegrationsController(IVcsService vcsService)
        {
            _vcsService = vcsService;
        }

        /// <summary>
        /// Adds a repository integration to a project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <param name="dto">The repository addition data.</param>
        /// <returns>The added repository details.</returns>
        [HttpPost("repositories")]
        public async Task<IActionResult> AddRepository(Guid projectId, [FromBody] AddRepositoryDto dto)
        {
            try
            {
                var result = await _vcsService.AddRepositoryAsync(projectId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all repositories integrated with a project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <returns>A list of integrated repositories.</returns>
        [HttpGet("repositories")]
        public async Task<IActionResult> GetRepositories(Guid projectId)
        {
            var result = await _vcsService.GetRepositoriesAsync(projectId);
            return Ok(result);
        }
    }
}

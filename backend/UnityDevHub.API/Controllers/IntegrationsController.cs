using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnityDevHub.API.Models.DevOps;
using UnityDevHub.API.Services;

namespace UnityDevHub.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/projects/{projectId}/integrations")]
    public class IntegrationsController : ControllerBase
    {
        private readonly IVcsService _vcsService;

        public IntegrationsController(IVcsService vcsService)
        {
            _vcsService = vcsService;
        }

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

        [HttpGet("repositories")]
        public async Task<IActionResult> GetRepositories(Guid projectId)
        {
            var result = await _vcsService.GetRepositoriesAsync(projectId);
            return Ok(result);
        }
    }
}

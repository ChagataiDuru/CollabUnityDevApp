using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UnityDevHub.API.Models.Documentation;
using UnityDevHub.API.Services;

namespace UnityDevHub.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class DocumentationController : ControllerBase
    {
        private readonly IDocumentationService _documentationService;

        public DocumentationController(IDocumentationService documentationService)
        {
            _documentationService = documentationService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        [HttpGet("documentation/search")]
        public async Task<ActionResult<IEnumerable<SearchResultDto>>> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query cannot be empty");

            var userId = GetCurrentUserId();
            var results = await _documentationService.SearchUnityDocsAsync(query, userId);
            return Ok(results);
        }

        [HttpGet("projects/{projectId}/documentation/pinned")]
        public async Task<ActionResult<IEnumerable<PinnedDocDto>>> GetPinnedDocs(Guid projectId)
        {
            var docs = await _documentationService.GetPinnedDocsAsync(projectId);
            return Ok(docs);
        }

        [HttpPost("projects/{projectId}/documentation/pinned")]
        public async Task<ActionResult<PinnedDocDto>> PinDoc(Guid projectId, [FromBody] CreatePinnedDocDto dto)
        {
            var userId = GetCurrentUserId();
            var doc = await _documentationService.PinDocAsync(projectId, userId, dto);
            return CreatedAtAction(nameof(GetPinnedDocs), new { projectId }, doc);
        }

        [HttpDelete("documentation/pinned/{id}")]
        public async Task<IActionResult> UnpinDoc(Guid id)
        {
            var userId = GetCurrentUserId();
            await _documentationService.UnpinDocAsync(id, userId);
            return NoContent();
        }

        [HttpGet("documentation/history")]
        public async Task<ActionResult<IEnumerable<SearchHistoryDto>>> GetHistory()
        {
            var userId = GetCurrentUserId();
            var history = await _documentationService.GetSearchHistoryAsync(userId);
            return Ok(history);
        }
    }
}

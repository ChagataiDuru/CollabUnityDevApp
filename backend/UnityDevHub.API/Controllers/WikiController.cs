using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnityDevHub.API.Data;
using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Models.Wiki;

namespace UnityDevHub.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class WikiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WikiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<List<WikiPageDto>>> GetProjectWiki(Guid projectId)
        {
            var pages = await _context.WikiPages
                .Include(w => w.LastEditor)
                .Where(w => w.ProjectId == projectId)
                .OrderBy(w => w.Title)
                .ToListAsync();

            // Transform to tree structure
            var rootPages = pages.Where(w => w.ParentId == null).Select(w => MapToDto(w, pages)).ToList();

            return Ok(rootPages);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WikiPageDto>> GetPage(Guid id)
        {
            var page = await _context.WikiPages
                .Include(w => w.LastEditor)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (page == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(page));
        }

        [HttpPost("project/{projectId}")]
        public async Task<ActionResult<WikiPageDto>> CreatePage(Guid projectId, CreateWikiPageDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            // Check if project exists
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);
            if (!projectExists)
            {
                return NotFound("Project not found");
            }

            // If ParentId is provided, verify it belongs to the same project
            if (dto.ParentId.HasValue)
            {
                var parentExists = await _context.WikiPages
                    .AnyAsync(w => w.Id == dto.ParentId && w.ProjectId == projectId);
                if (!parentExists)
                {
                    return BadRequest("Parent page not found in this project");
                }
            }

            var page = new WikiPage
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                ParentId = dto.ParentId,
                Title = dto.Title,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastEditorId = userId
            };

            _context.WikiPages.Add(page);
            await _context.SaveChangesAsync();

            // Reload to get relations if needed, or just map manually
            return CreatedAtAction(nameof(GetPage), new { id = page.Id }, MapToDto(page));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<WikiPageDto>> UpdatePage(Guid id, UpdateWikiPageDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var page = await _context.WikiPages.FindAsync(id);
            if (page == null)
            {
                return NotFound();
            }

            // Verify ParentId if changing (avoid circular ref would be good too, but basic check for now)
            if (dto.ParentId.HasValue && dto.ParentId != page.ParentId)
            {
                if (dto.ParentId == id)
                {
                    return BadRequest("Page cannot be its own parent");
                }

                var parentExists = await _context.WikiPages
                    .AnyAsync(w => w.Id == dto.ParentId && w.ProjectId == page.ProjectId);
                if (!parentExists)
                {
                    return BadRequest("Parent page not found in this project");
                }
            }

            page.Title = dto.Title;
            page.Content = dto.Content;
            page.ParentId = dto.ParentId;
            page.UpdatedAt = DateTime.UtcNow;
            page.LastEditorId = userId;

            await _context.SaveChangesAsync();

            return Ok(MapToDto(page));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePage(Guid id)
        {
            var page = await _context.WikiPages.Include(w => w.Children).FirstOrDefaultAsync(w => w.Id == id);
            if (page == null)
            {
                return NotFound();
            }

            // Option: Cascade delete children or move them to root?
            // DB Cascade delete might handle it if configured, but we used NoAction for Parent/Children to avoid cycles.
            // So we must handle children.
            // Requirement says hierarchical. If I delete a parent, children should probably be deleted or moved up.
            // Let's delete children recursively for now (simple approach) or just let them become root.
            // Let's make them root (ParentId = null) to be safe against accidental data loss.

            foreach (var child in page.Children)
            {
                child.ParentId = null;
            }

            _context.WikiPages.Remove(page);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private WikiPageDto MapToDto(WikiPage page, List<WikiPage>? allPages = null)
        {
            var dto = new WikiPageDto
            {
                Id = page.Id,
                ProjectId = page.ProjectId,
                ParentId = page.ParentId,
                Title = page.Title,
                Content = page.Content,
                CreatedAt = page.CreatedAt,
                UpdatedAt = page.UpdatedAt,
                LastEditorId = page.LastEditorId,
                LastEditorName = page.LastEditor?.FullName ?? "Unknown" // Assuming User has FullName or similar
            };

            if (allPages != null)
            {
                dto.Children = allPages
                    .Where(w => w.ParentId == page.Id)
                    .Select(w => MapToDto(w, allPages))
                    .ToList();
            }

            return dto;
        }
    }
}

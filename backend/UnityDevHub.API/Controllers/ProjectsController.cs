using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnityDevHub.API.Data;
using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Models.Project;

namespace UnityDevHub.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Controller for managing projects.
/// </summary>
public class ProjectsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProjectsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all projects for the current user.
    /// </summary>
    /// <returns>A list of projects the user is a member of.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Get all projects where user is a member
        var projectIds = await _context.ProjectMembers
            .Where(pm => pm.UserId == userId)
            .Select(pm => pm.ProjectId)
            .ToListAsync();

        var projects = await _context.Projects
            .Where(p => projectIds.Contains(p.Id))
            .Include(p => p.CreatedBy)
            .OrderByDescending(p => p.UpdatedAt)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ColorTheme = p.ColorTheme,
                CreatedById = p.CreatedById,
                CreatedByName = p.CreatedBy != null ? p.CreatedBy.DisplayName : null,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return Ok(projects);
    }

    /// <summary>
    /// Retrieves a specific project by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the project.</param>
    /// <returns>The project details if found and user has access.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Check if user is a member
        var isMember = await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == id && pm.UserId == userId);

        if (!isMember)
        {
            return Forbid();
        }

        var project = await _context.Projects
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return NotFound();
        }

        return Ok(new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            ColorTheme = project.ColorTheme,
            CreatedById = project.CreatedById,
            CreatedByName = project.CreatedBy?.DisplayName,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        });
    }

    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <param name="dto">The project creation data.</param>
    /// <returns>The created project.</returns>
    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            ColorTheme = dto.ColorTheme,
            CreatedById = userId
        };

        _context.Projects.Add(project);
        
        // Create default columns
        var columns = new List<TaskColumn>
        {
            new TaskColumn { ProjectId = project.Id, Name = "To Do", Position = 0, Color = "#64748b" },
            new TaskColumn { ProjectId = project.Id, Name = "In Progress", Position = 1, Color = "#3b82f6" },
            new TaskColumn { ProjectId = project.Id, Name = "Done", Position = 2, Color = "#10b981" }
        };
        
        _context.TaskColumns.AddRange(columns);

        // Auto-create Owner membership
        var ownerMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = userId,
            Role = ProjectRole.Owner
        };
        _context.ProjectMembers.Add(ownerMember);
        
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            ColorTheme = project.ColorTheme,
            CreatedById = project.CreatedById,
            CreatedByName = User.Identity?.Name,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        });
    }

    /// <summary>
    /// Updates an existing project.
    /// </summary>
    /// <param name="id">The unique identifier of the project to update.</param>
    /// <param name="dto">The project update data.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(Guid id, UpdateProjectDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Check permissions (Owner or Admin)
        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == id && pm.UserId == userId);

        if (member == null || (member.Role != ProjectRole.Owner && member.Role != ProjectRole.Admin))
        {
            return Forbid();
        }

        var project = await _context.Projects.FindAsync(id);

        if (project == null)
        {
            return NotFound();
        }

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.ColorTheme = dto.ColorTheme;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Deletes a project.
    /// </summary>
    /// <param name="id">The unique identifier of the project to delete.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Only Owner can delete
        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == id && pm.UserId == userId);

        if (member == null || member.Role != ProjectRole.Owner)
        {
            return Forbid();
        }

        var project = await _context.Projects.FindAsync(id);

        if (project == null)
        {
            return NotFound();
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

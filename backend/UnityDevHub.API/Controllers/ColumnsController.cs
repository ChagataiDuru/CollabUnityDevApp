using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UnityDevHub.API.Data;
using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Hubs;
using UnityDevHub.API.Models.TaskColumn;

namespace UnityDevHub.API.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public class ColumnsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<ProjectHub> _projectHub;

    public ColumnsController(ApplicationDbContext context, IHubContext<ProjectHub> projectHub)
    {
        _context = context;
        _projectHub = projectHub;
    }

    [HttpGet("projects/{projectId}/columns")]
    public async Task<ActionResult<IEnumerable<ColumnDto>>> GetColumns(Guid projectId)
    {
        var columns = await _context.TaskColumns
            .Where(c => c.ProjectId == projectId)
            .OrderBy(c => c.Position)
            .Select(c => new ColumnDto
            {
                Id = c.Id,
                ProjectId = c.ProjectId,
                Name = c.Name,
                Position = c.Position,
                Color = c.Color
            })
            .ToListAsync();

        return Ok(columns);
    }

    [HttpPost("projects/{projectId}/columns")]
    public async Task<ActionResult<ColumnDto>> CreateColumn(Guid projectId, CreateColumnDto dto)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null) return NotFound("Project not found");

        // Get max position
        var maxPos = await _context.TaskColumns
            .Where(c => c.ProjectId == projectId)
            .MaxAsync(c => (int?)c.Position) ?? -1;

        var column = new TaskColumn
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = dto.Name,
            Color = dto.Color,
            Position = maxPos + 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.TaskColumns.Add(column);
        await _context.SaveChangesAsync();

        var columnDto = new ColumnDto
        {
            Id = column.Id,
            ProjectId = column.ProjectId,
            Name = column.Name,
            Position = column.Position,
            Color = column.Color
        };

        // Broadcast update
        await _projectHub.Clients.Group(projectId.ToString()).SendAsync("ColumnCreated", columnDto);

        return CreatedAtAction(nameof(GetColumns), new { projectId }, columnDto);
    }

    [HttpPut("columns/{id}")]
    public async Task<IActionResult> UpdateColumn(Guid id, UpdateColumnDto dto)
    {
        var column = await _context.TaskColumns.FindAsync(id);
        if (column == null) return NotFound();

        column.Name = dto.Name;
        column.Color = dto.Color;

        await _context.SaveChangesAsync();

        var columnDto = new ColumnDto
        {
            Id = column.Id,
            ProjectId = column.ProjectId,
            Name = column.Name,
            Position = column.Position,
            Color = column.Color
        };

        await _projectHub.Clients.Group(column.ProjectId.ToString()).SendAsync("ColumnUpdated", columnDto);

        return Ok(columnDto);
    }

    [HttpDelete("columns/{id}")]
    public async Task<IActionResult> DeleteColumn(Guid id)
    {
        var column = await _context.TaskColumns.FindAsync(id);
        if (column == null) return NotFound();

        var projectId = column.ProjectId;
        _context.TaskColumns.Remove(column);
        await _context.SaveChangesAsync();

        await _projectHub.Clients.Group(projectId.ToString()).SendAsync("ColumnDeleted", id);

        return NoContent();
    }

    [HttpPut("columns/reorder")]
    public async Task<IActionResult> ReorderColumns([FromBody] List<Guid> columnIds)
    {
        if (!columnIds.Any()) return BadRequest();

        var columns = await _context.TaskColumns
            .Where(c => columnIds.Contains(c.Id))
            .ToListAsync();

        if (!columns.Any()) return NotFound();

        var projectId = columns.First().ProjectId;

        foreach (var column in columns)
        {
            var index = columnIds.IndexOf(column.Id);
            if (index != -1)
            {
                column.Position = index;
            }
        }

        await _context.SaveChangesAsync();

        await _projectHub.Clients.Group(projectId.ToString()).SendAsync("ColumnsReordered", columnIds);

        return NoContent();
    }
}

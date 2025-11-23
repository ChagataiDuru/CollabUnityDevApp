using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnityDevHub.API.Data;
using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Models.Sprint;

namespace UnityDevHub.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    /// <summary>
    /// Controller for managing project sprints.
    /// </summary>
    public class SprintsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SprintsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all sprints for a specific project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <returns>A list of sprints associated with the project.</returns>
        [HttpGet("projects/{projectId}/sprints")]
        public async Task<ActionResult<IEnumerable<SprintDto>>> GetSprints(Guid projectId)
        {
            var sprints = await _context.Sprints
                .Where(s => s.ProjectId == projectId)
                .Select(s => new SprintDto
                {
                    Id = s.Id,
                    ProjectId = s.ProjectId,
                    Name = s.Name,
                    Description = s.Description,
                    Goal = s.Goal,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Status = s.Status,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    TaskCount = s.Tasks.Count,
                    CompletedTaskCount = s.Tasks.Count(t => t.Column!.Name == "Done" || t.Column!.Name == "Completed")
                })
                .OrderBy(s => s.StartDate)
                .ToListAsync();

            return Ok(sprints);
        }

        /// <summary>
        /// Retrieves a specific sprint by ID.
        /// </summary>
        /// <param name="id">The unique identifier of the sprint.</param>
        /// <returns>The sprint details if found.</returns>
        [HttpGet("sprints/{id}")]
        public async Task<ActionResult<SprintDto>> GetSprint(Guid id)
        {
            var sprint = await _context.Sprints
                .Include(s => s.Tasks)
                    .ThenInclude(t => t.Column)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sprint == null) return NotFound();

            var dto = new SprintDto
            {
                Id = sprint.Id,
                ProjectId = sprint.ProjectId,
                Name = sprint.Name,
                Description = sprint.Description,
                Goal = sprint.Goal,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                Status = sprint.Status,
                CreatedAt = sprint.CreatedAt,
                UpdatedAt = sprint.UpdatedAt,
                TaskCount = sprint.Tasks.Count,
                CompletedTaskCount = sprint.Tasks.Count(t => t.Column?.Name == "Done" || t.Column?.Name == "Completed")
            };

            return Ok(dto);
        }

        /// <summary>
        /// Creates a new sprint in a project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <param name="dto">The sprint creation data.</param>
        /// <returns>The created sprint.</returns>
        [HttpPost("projects/{projectId}/sprints")]
        public async Task<ActionResult<SprintDto>> CreateSprint(Guid projectId, CreateSprintDto dto)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound("Project not found");

            var sprint = new Data.Entities.Sprint
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = dto.Name,
                Description = dto.Description,
                Goal = dto.Goal,
                StartDate = dto.StartDate.ToUniversalTime(),
                EndDate = dto.EndDate.ToUniversalTime(),
                Status = SprintStatus.Planning,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();

            var sprintDto = new SprintDto
            {
                Id = sprint.Id,
                ProjectId = sprint.ProjectId,
                Name = sprint.Name,
                Description = sprint.Description,
                Goal = sprint.Goal,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                Status = sprint.Status,
                CreatedAt = sprint.CreatedAt,
                UpdatedAt = sprint.UpdatedAt,
                TaskCount = 0,
                CompletedTaskCount = 0
            };

            return CreatedAtAction(nameof(GetSprint), new { id = sprint.Id }, sprintDto);
        }

        /// <summary>
        /// Updates an existing sprint.
        /// </summary>
        /// <param name="id">The unique identifier of the sprint to update.</param>
        /// <param name="dto">The sprint update data.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("sprints/{id}")]
        public async Task<IActionResult> UpdateSprint(Guid id, UpdateSprintDto dto)
        {
            var sprint = await _context.Sprints.FindAsync(id);
            if (sprint == null) return NotFound();

            sprint.Name = dto.Name;
            sprint.Description = dto.Description;
            sprint.Goal = dto.Goal;
            sprint.StartDate = dto.StartDate.ToUniversalTime();
            sprint.EndDate = dto.EndDate.ToUniversalTime();
            sprint.Status = dto.Status;
            sprint.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Deletes a sprint.
        /// </summary>
        /// <param name="id">The unique identifier of the sprint to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("sprints/{id}")]
        public async Task<IActionResult> DeleteSprint(Guid id)
        {
            var sprint = await _context.Sprints.FindAsync(id);
            if (sprint == null) return NotFound();

            _context.Sprints.Remove(sprint);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Retrieves burndown chart data for a sprint.
        /// </summary>
        /// <param name="id">The unique identifier of the sprint.</param>
        /// <returns>A list of burndown data points.</returns>
        [HttpGet("sprints/{id}/burndown")]
        public async Task<ActionResult<IEnumerable<BurndownDataDto>>> GetBurndownData(Guid id)
        {
            var sprint = await _context.Sprints
                .Include(s => s.Tasks)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sprint == null) return NotFound();

            var totalTasks = sprint.Tasks.Count;
            var sprintDurationDays = (sprint.EndDate - sprint.StartDate).Days + 1;

            // Generate burndown data
            var burndownData = new List<BurndownDataDto>();

            for (int i = 0; i < sprintDurationDays; i++)
            {
                var currentDate = sprint.StartDate.AddDays(i);
                
                // Count tasks completed before or on current date
                var completedByDate = sprint.Tasks.Count(t => 
                    t.UpdatedAt <= currentDate && 
                    (t.Column?.Name == "Done" || t.Column?.Name == "Completed")
                );

                var remainingTasks = totalTasks - completedByDate;
                var idealRemaining = totalTasks - (totalTasks * i / sprintDurationDays);

                burndownData.Add(new BurndownDataDto
                {
                    Date = currentDate,
                    RemainingTasks = remainingTasks,
                    IdealRemainingTasks = idealRemaining
                });
            }

            return Ok(burndownData);
        }
    }
}

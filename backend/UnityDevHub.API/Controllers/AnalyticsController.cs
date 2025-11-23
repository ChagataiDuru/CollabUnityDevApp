using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnityDevHub.API.Data;
using UnityDevHub.API.Models.Analytics; // We will create this namespace/models next

namespace UnityDevHub.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/projects/{projectId}/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("completion-rate")]
        public async Task<ActionResult<CompletionRateDto>> GetCompletionRate(Guid projectId)
        {
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);
            if (!projectExists)
            {
                return NotFound("Project not found");
            }

            var totalTasks = await _context.Tasks.CountAsync(t => t.ProjectId == projectId);

            if (totalTasks == 0)
            {
                return Ok(new CompletionRateDto { TotalTasks = 0, CompletedTasks = 0, RatePercentage = 0 });
            }

            // Using EF Core functions to filter in DB
            var completedTasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId && t.Column != null &&
                            (t.Column.Name.ToLower().Contains("done") ||
                             t.Column.Name.ToLower().Contains("completed") ||
                             t.Column.Name.ToLower().Contains("closed")))
                .CountAsync();

            var rate = (double)completedTasks / totalTasks * 100;

            return Ok(new CompletionRateDto
            {
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                RatePercentage = Math.Round(rate, 2)
            });
        }

        [HttpGet("activity-heatmap")]
        public async Task<ActionResult<List<HeatmapPointDto>>> GetActivityHeatmap(Guid projectId, [FromQuery] int? year)
        {
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);
            if (!projectExists)
            {
                return NotFound("Project not found");
            }

            int targetYear = year ?? DateTime.UtcNow.Year;
            var startDate = new DateTime(targetYear, 1, 1).ToUniversalTime();
            var endDate = new DateTime(targetYear, 12, 31, 23, 59, 59).ToUniversalTime();

            // 1. Task Creation
            var taskActivity = await _context.Tasks
                .Where(t => t.ProjectId == projectId && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // 2. Wiki Edits (Created or Updated)
            // Note: UpdatedAt changes on every edit, losing history of previous edits on the same page unless we had a history table.
            // We will count CreatedAt and UpdatedAt (if different day) as approximation.
            // Ideally we'd query an audit log. For now, CreatedAt + UpdatedAt is the best we can do with current schema.
            var wikiCreation = await _context.WikiPages
                .Where(w => w.ProjectId == projectId && w.CreatedAt >= startDate && w.CreatedAt <= endDate)
                .GroupBy(w => w.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

             var wikiUpdates = await _context.WikiPages
                .Where(w => w.ProjectId == projectId && w.UpdatedAt >= startDate && w.UpdatedAt <= endDate && w.UpdatedAt.Date != w.CreatedAt.Date)
                .GroupBy(w => w.UpdatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // 3. Commits (Linked to Project via Repository)
            // Commits are linked to Repository, Repository linked to Project.
            var commitActivity = await _context.Commits
                .Where(c => c.Repository.ProjectId == projectId && c.Timestamp >= startDate && c.Timestamp <= endDate)
                .GroupBy(c => c.Timestamp.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // Aggregate
            var heatmapData = new Dictionary<DateTime, int>();

            void AddCounts(IEnumerable<dynamic> items)
            {
                foreach (var item in items)
                {
                    if (!heatmapData.ContainsKey(item.Date))
                    {
                        heatmapData[item.Date] = 0;
                    }
                    heatmapData[item.Date] += item.Count;
                }
            }

            AddCounts(taskActivity);
            AddCounts(wikiCreation);
            AddCounts(wikiUpdates);
            AddCounts(commitActivity);

            var result = heatmapData
                .Select(kv => new HeatmapPointDto { Date = kv.Key, Count = kv.Value })
                .OrderBy(x => x.Date)
                .ToList();

            return Ok(result);
        }
    }
}

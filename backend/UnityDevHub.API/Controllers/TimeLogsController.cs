using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UnityDevHub.API.Data;
using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Models.TimeLog;

namespace UnityDevHub.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class TimeLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TimeLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        [HttpGet("tasks/{taskId}/timelogs")]
        public async Task<ActionResult<IEnumerable<TimeLogDto>>> GetTimeLogs(Guid taskId)
        {
            var timeLogs = await _context.TimeLogs
                .Include(tl => tl.User)
                .Where(tl => tl.TaskId == taskId)
                .OrderByDescending(tl => tl.CreatedAt)
                .Select(tl => new TimeLogDto
                {
                    Id = tl.Id,
                    TaskId = tl.TaskId,
                    UserId = tl.UserId,
                    UserName = tl.User.DisplayName ?? tl.User.Username,
                    StartTime = tl.StartTime,
                    EndTime = tl.EndTime,
                    DurationMinutes = tl.DurationMinutes,
                    Description = tl.Description,
                    IsManual = tl.IsManual,
                    CreatedAt = tl.CreatedAt
                })
                .ToListAsync();

            return Ok(timeLogs);
        }

        [HttpPost("tasks/{taskId}/timelogs/start")]
        public async Task<ActionResult<TimeLogDto>> StartTimer(Guid taskId, StartTimerDto dto)
        {
            var userId = GetCurrentUserId();

            // Check if there's already an active timer for this user
            var activeTimer = await _context.TimeLogs
                .FirstOrDefaultAsync(tl => tl.UserId == userId && tl.EndTime == null);

            if (activeTimer != null)
            {
                return BadRequest("You already have an active timer running. Stop it before starting a new one.");
            }

            var timeLog = new Data.Entities.TimeLog
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                UserId = userId,
                StartTime = DateTime.UtcNow,
                EndTime = null,
                DurationMinutes = 0,
                Description = dto.Description,
                IsManual = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.TimeLogs.Add(timeLog);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return Ok(new TimeLogDto
            {
                Id = timeLog.Id,
                TaskId = timeLog.TaskId,
                UserId = timeLog.UserId,
                UserName = user?.DisplayName ?? user?.Username ?? "Unknown",
                StartTime = timeLog.StartTime,
                EndTime = timeLog.EndTime,
                DurationMinutes = timeLog.DurationMinutes,
                Description = timeLog.Description,
                IsManual = timeLog.IsManual,
                CreatedAt = timeLog.CreatedAt
            });
        }

        [HttpPut("timelogs/{id}/stop")]
        public async Task<ActionResult<TimeLogDto>> StopTimer(Guid id)
        {
            var userId = GetCurrentUserId();
            var timeLog = await _context.TimeLogs
                .Include(tl => tl.User)
                .FirstOrDefaultAsync(tl => tl.Id == id && tl.UserId == userId);

            if (timeLog == null) return NotFound();

            if (timeLog.EndTime != null)
            {
                return BadRequest("This timer has already been stopped.");
            }

            timeLog.EndTime = DateTime.UtcNow;
            timeLog.DurationMinutes = (int)(timeLog.EndTime.Value - timeLog.StartTime!.Value).TotalMinutes;

            await _context.SaveChangesAsync();

            return Ok(new TimeLogDto
            {
                Id = timeLog.Id,
                TaskId = timeLog.TaskId,
                UserId = timeLog.UserId,
                UserName = timeLog.User.DisplayName ?? timeLog.User.Username,
                StartTime = timeLog.StartTime,
                EndTime = timeLog.EndTime,
                DurationMinutes = timeLog.DurationMinutes,
                Description = timeLog.Description,
                IsManual = timeLog.IsManual,
                CreatedAt = timeLog.CreatedAt
            });
        }

        [HttpPost("tasks/{taskId}/timelogs/manual")]
        public async Task<ActionResult<TimeLogDto>> CreateManualTimeLog(Guid taskId, CreateManualTimeLogDto dto)
        {
            var userId = GetCurrentUserId();

            var durationMinutes = (int)(dto.EndTime - dto.StartTime).TotalMinutes;

            if (durationMinutes <= 0)
            {
                return BadRequest("End time must be after start time.");
            }

            var timeLog = new Data.Entities.TimeLog
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                UserId = userId,
                StartTime = dto.StartTime.ToUniversalTime(),
                EndTime = dto.EndTime.ToUniversalTime(),
                DurationMinutes = durationMinutes,
                Description = dto.Description,
                IsManual = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.TimeLogs.Add(timeLog);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return CreatedAtAction(nameof(GetTimeLogs), new { taskId }, new TimeLogDto
            {
                Id = timeLog.Id,
                TaskId = timeLog.TaskId,
                UserId = timeLog.UserId,
                UserName = user?.DisplayName ?? user?.Username ?? "Unknown",
                StartTime = timeLog.StartTime,
                EndTime = timeLog.EndTime,
                DurationMinutes = timeLog.DurationMinutes,
                Description = timeLog.Description,
                IsManual = timeLog.IsManual,
                CreatedAt = timeLog.CreatedAt
            });
        }

        [HttpDelete("timelogs/{id}")]
        public async Task<IActionResult> DeleteTimeLog(Guid id)
        {
            var userId = GetCurrentUserId();
            var timeLog = await _context.TimeLogs
                .FirstOrDefaultAsync(tl => tl.Id == id && tl.UserId == userId);

            if (timeLog == null) return NotFound();

            _context.TimeLogs.Remove(timeLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("projects/{projectId}/timelogs/report")]
        public async Task<ActionResult> GetTimeReport(Guid projectId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var query = _context.TimeLogs
                .Include(tl => tl.User)
                .Include(tl => tl.Task)
                .Where(tl => tl.Task.ProjectId == projectId);

            if (startDate.HasValue)
            {
                query = query.Where(tl => tl.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(tl => tl.CreatedAt <= endDate.Value);
            }

            var timeLogs = await query.ToListAsync();

            var report = new
            {
                TotalMinutes = timeLogs.Sum(tl => tl.DurationMinutes),
                TotalHours = Math.Round(timeLogs.Sum(tl => tl.DurationMinutes) / 60.0, 2),
                ByUser = timeLogs.GroupBy(tl => new { tl.UserId, UserName = tl.User.DisplayName ?? tl.User.Username })
                    .Select(g => new
                    {
                        UserId = g.Key.UserId,
                        UserName = g.Key.UserName,
                        TotalMinutes = g.Sum(tl => tl.DurationMinutes),
                        TotalHours = Math.Round(g.Sum(tl => tl.DurationMinutes) / 60.0, 2)
                    }),
                ByTask = timeLogs.GroupBy(tl => new { tl.TaskId, TaskTitle = tl.Task.Title })
                    .Select(g => new
                    {
                        TaskId = g.Key.TaskId,
                        TaskTitle = g.Key.TaskTitle,
                        TotalMinutes = g.Sum(tl => tl.DurationMinutes),
                        TotalHours = Math.Round(g.Sum(tl => tl.DurationMinutes) / 60.0, 2)
                    })
            };

            return Ok(report);
        }
    }
}

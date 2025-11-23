using System.Security.Claims;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UnityDevHub.API.Data;
using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Hubs;
using UnityDevHub.API.Models.Task;
using UnityDevHub.API.Models.Notification;

namespace UnityDevHub.API.Controllers;

[Authorize]
[ApiController]
[Route("api")]
/// <summary>
/// Controller for managing tasks and their related entities.
/// </summary>
public class TasksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<ProjectHub> _projectHub;

    public TasksController(ApplicationDbContext context, IHubContext<ProjectHub> projectHub)
    {
        _context = context;
        _projectHub = projectHub;
    }

    /// <summary>
    /// Retrieves all tasks for a specific project.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <returns>A list of tasks associated with the project.</returns>
    [HttpGet("projects/{projectId}/tasks")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks(Guid projectId)
    {
        var tasks = await _context.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.Tags)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.ChecklistItems)
            .Include(t => t.Commits)
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.Position)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                ColumnId = t.ColumnId,
                Title = t.Title,
                Description = t.Description,
                AssignedToId = t.AssignedToId,
                AssignedToName = t.AssignedTo != null ? t.AssignedTo.DisplayName : null,
                AssignedToAvatar = t.AssignedTo != null ? t.AssignedTo.AvatarUrl : null,
                Priority = t.Priority,
                DueDate = t.DueDate,
                EstimatedHours = t.EstimatedHours,
                Position = t.Position,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CommentsCount = t.Comments.Count,
                AttachmentsCount = t.Attachments.Count,
                ChecklistTotal = t.ChecklistItems.Count,
                ChecklistCompleted = t.ChecklistItems.Count(ci => ci.IsCompleted),
                TaskNumber = t.TaskNumber,
                Tags = t.Tags.Select(tag => new TaskTagDto { Id = tag.Id, Name = tag.Name, Color = tag.Color }).ToList(),
                Commits = t.Commits.Select(c => new CommitDto { Id = c.Id, Hash = c.Hash, Message = c.Message, AuthorName = c.AuthorName, Timestamp = c.Timestamp, Url = c.Url }).ToList()
            })
            .ToListAsync();

        return Ok(tasks);
    }

    /// <summary>
    /// Retrieves a specific task by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the task.</param>
    /// <returns>The task details if found.</returns>
    [HttpGet("tasks/{id}")]
    public async Task<ActionResult<TaskDto>> GetTask(Guid id)
    {
        var task = await _context.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.Tags)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.ChecklistItems)
            .Include(t => t.Commits)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null) return NotFound();

        return Ok(new TaskDto
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            ColumnId = task.ColumnId,
            Title = task.Title,
            Description = task.Description,
            AssignedToId = task.AssignedToId,
            AssignedToName = task.AssignedTo?.DisplayName,
            AssignedToAvatar = task.AssignedTo?.AvatarUrl,
            Priority = task.Priority,
            DueDate = task.DueDate,
            EstimatedHours = task.EstimatedHours,
            Position = task.Position,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CommentsCount = task.Comments.Count,
            AttachmentsCount = task.Attachments.Count,
            ChecklistTotal = task.ChecklistItems.Count,
            ChecklistCompleted = task.ChecklistItems.Count(ci => ci.IsCompleted),
            Tags = task.Tags.Select(tag => new TaskTagDto { Id = tag.Id, Name = tag.Name, Color = tag.Color }).ToList(),
            ChecklistItems = task.ChecklistItems.Select(ci => new ChecklistItemDto { Id = ci.Id, Title = ci.Text, IsCompleted = ci.IsCompleted, Position = ci.Position }).OrderBy(ci => ci.Position).ToList(),
            Comments = task.Comments.Select(c => new TaskCommentDto { Id = c.Id, Content = c.Content, CreatedAt = c.CreatedAt, UserId = c.UserId ?? Guid.Empty, UserName = c.User?.DisplayName ?? "Unknown", UserAvatar = c.User != null ? c.User.AvatarUrl : null }).OrderByDescending(c => c.CreatedAt).ToList(),
            Attachments = task.Attachments.Select(a => new TaskAttachmentDto { Id = a.Id, FileName = a.FileName, FilePath = a.FilePath, UploadedAt = a.UploadedAt, UploadedById = a.UploadedById ?? Guid.Empty, UploadedByName = a.UploadedBy?.DisplayName ?? "Unknown" }).OrderByDescending(a => a.UploadedAt).ToList(),
            TaskNumber = task.TaskNumber,
            Commits = task.Commits.Select(c => new CommitDto { Id = c.Id, Hash = c.Hash, Message = c.Message, AuthorName = c.AuthorName, Timestamp = c.Timestamp, Url = c.Url }).OrderByDescending(c => c.Timestamp).ToList()
        });
    }

    /// <summary>
    /// Creates a new task in a project.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="dto">The task creation data.</param>
    /// <returns>The created task.</returns>
    [HttpPost("projects/{projectId}/tasks")]
    public async Task<ActionResult<TaskDto>> CreateTask(Guid projectId, CreateTaskDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Get max position in column
        var maxPos = await _context.Tasks
            .Where(t => t.ColumnId == dto.ColumnId)
            .MaxAsync(t => (int?)t.Position) ?? -1;

        // Get max task number for project
        var maxTaskNum = await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .MaxAsync(t => (int?)t.TaskNumber) ?? 0;

        var task = new ProjectTask
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            ColumnId = dto.ColumnId,
            Title = dto.Title,
            Description = dto.Description,
            AssignedToId = dto.AssignedToId,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            EstimatedHours = dto.EstimatedHours,
            Position = maxPos + 1,
            TaskNumber = maxTaskNum + 1,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Fetch to get included data (like AssignedTo)
        var createdTask = await _context.Tasks
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == task.Id);

        var taskDto = new TaskDto
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            ColumnId = task.ColumnId,
            Title = task.Title,
            Description = task.Description,
            AssignedToId = task.AssignedToId,
            AssignedToName = createdTask?.AssignedTo?.DisplayName,
            AssignedToAvatar = createdTask?.AssignedTo?.AvatarUrl,
            Priority = task.Priority,
            DueDate = task.DueDate,
            EstimatedHours = task.EstimatedHours,
            Position = task.Position,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CommentsCount = 0,
            AttachmentsCount = 0,
            ChecklistTotal = 0,
            ChecklistCompleted = 0,
            Tags = new List<TaskTagDto>(),
            ChecklistItems = new List<ChecklistItemDto>(),
            Comments = new List<TaskCommentDto>(),
            Attachments = new List<TaskAttachmentDto>(),
            TaskNumber = task.TaskNumber,
            Commits = new List<CommitDto>()
        };

        await _projectHub.Clients.Group(projectId.ToString()).SendAsync("TaskCreated", taskDto);

        // Notification: Task Assignment
        if (task.AssignedToId.HasValue && task.AssignedToId != userId)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = task.AssignedToId.Value,
                Title = "New Task Assignment",
                Message = $"You have been assigned to task: {task.Title}",
                Type = "Info",
                RelatedEntityId = task.Id,
                RelatedEntityType = "Task",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                RelatedEntityId = notification.RelatedEntityId,
                RelatedEntityType = notification.RelatedEntityType
            };

            await _projectHub.Clients.User(task.AssignedToId.Value.ToString()).SendAsync("NotificationReceived", notificationDto);
        }

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, taskDto);
    }

    /// <summary>
    /// Updates an existing task.
    /// </summary>
    /// <param name="id">The unique identifier of the task to update.</param>
    /// <param name="dto">The task update data.</param>
    /// <returns>The updated task details.</returns>
    [HttpPut("tasks/{id}")]
    public async Task<IActionResult> UpdateTask(Guid id, UpdateTaskDto dto)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        var oldAssignedToId = task.AssignedToId;

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.AssignedToId = dto.AssignedToId;
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.EstimatedHours = dto.EstimatedHours;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Notification: Task Assignment Change
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (task.AssignedToId.HasValue && task.AssignedToId != oldAssignedToId && task.AssignedToId != currentUserId)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = task.AssignedToId.Value,
                Title = "Task Assignment",
                Message = $"You have been assigned to task: {task.Title}",
                Type = "Info",
                RelatedEntityId = task.Id,
                RelatedEntityType = "Task",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                RelatedEntityId = notification.RelatedEntityId,
                RelatedEntityType = notification.RelatedEntityType
            };

            await _projectHub.Clients.User(task.AssignedToId.Value.ToString()).SendAsync("NotificationReceived", notificationDto);
        }

        // Fetch updated task with details
        var updatedTask = await _context.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.Tags)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.ChecklistItems)
            .FirstOrDefaultAsync(t => t.Id == id);

        var taskDto = new TaskDto
        {
            Id = updatedTask!.Id,
            ProjectId = updatedTask.ProjectId,
            ColumnId = updatedTask.ColumnId,
            Title = updatedTask.Title,
            Description = updatedTask.Description,
            AssignedToId = updatedTask.AssignedToId,
            AssignedToName = updatedTask.AssignedTo?.DisplayName,
            AssignedToAvatar = updatedTask.AssignedTo?.AvatarUrl,
            Priority = updatedTask.Priority,
            DueDate = updatedTask.DueDate,
            EstimatedHours = updatedTask.EstimatedHours,
            Position = updatedTask.Position,
            CreatedAt = updatedTask.CreatedAt,
            UpdatedAt = updatedTask.UpdatedAt,
            CommentsCount = updatedTask.Comments.Count,
            AttachmentsCount = updatedTask.Attachments.Count,
            ChecklistTotal = updatedTask.ChecklistItems.Count,
            ChecklistCompleted = updatedTask.ChecklistItems.Count(ci => ci.IsCompleted),
            Tags = updatedTask.Tags.Select(tag => new TaskTagDto { Id = tag.Id, Name = tag.Name, Color = tag.Color }).ToList(),
            ChecklistItems = updatedTask.ChecklistItems.Select(ci => new ChecklistItemDto { Id = ci.Id, Title = ci.Text, IsCompleted = ci.IsCompleted, Position = ci.Position }).OrderBy(ci => ci.Position).ToList(),
            Comments = updatedTask.Comments.Select(c => new TaskCommentDto { Id = c.Id, Content = c.Content, CreatedAt = c.CreatedAt, UserId = c.UserId ?? Guid.Empty, UserName = c.User?.DisplayName ?? "Unknown", UserAvatar = c.User != null ? c.User.AvatarUrl : null }).OrderByDescending(c => c.CreatedAt).ToList(),
            Attachments = updatedTask.Attachments.Select(a => new TaskAttachmentDto { Id = a.Id, FileName = a.FileName, FilePath = a.FilePath, UploadedAt = a.UploadedAt, UploadedById = a.UploadedById ?? Guid.Empty, UploadedByName = a.UploadedBy?.DisplayName ?? "Unknown" }).OrderByDescending(a => a.UploadedAt).ToList(),
            TaskNumber = updatedTask.TaskNumber,
            Commits = updatedTask.Commits.Select(c => new CommitDto { Id = c.Id, Hash = c.Hash, Message = c.Message, AuthorName = c.AuthorName, Timestamp = c.Timestamp, Url = c.Url }).OrderByDescending(c => c.Timestamp).ToList()
        };

        await _projectHub.Clients.Group(task.ProjectId.ToString()).SendAsync("TaskUpdated", taskDto);

        return Ok(taskDto);
    }

    /// <summary>
    /// Deletes a task.
    /// </summary>
    /// <param name="id">The unique identifier of the task to delete.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("tasks/{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        var projectId = task.ProjectId;
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        await _projectHub.Clients.Group(projectId.ToString()).SendAsync("TaskDeleted", id);

        return NoContent();
    }

    /// <summary>
    /// Moves a task to a different column or position.
    /// </summary>
    /// <param name="id">The unique identifier of the task to move.</param>
    /// <param name="dto">The move data including new column and position.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("tasks/{id}/move")]
    public async Task<IActionResult> MoveTask(Guid id, MoveTaskDto dto)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        var oldColumnId = task.ColumnId;
        var oldPosition = task.Position;
        var newColumnId = dto.NewColumnId;
        var newPosition = dto.NewPosition;

        if (oldColumnId == newColumnId)
        {
            // Reorder within same column
            var tasksInColumn = await _context.Tasks
                .Where(t => t.ColumnId == oldColumnId && t.Id != id)
                .OrderBy(t => t.Position)
                .ToListAsync();

            tasksInColumn.Insert(newPosition, task);

            for (int i = 0; i < tasksInColumn.Count; i++)
            {
                tasksInColumn[i].Position = i;
            }
        }
        else
        {
            // Move to different column
            // Shift tasks in old column
            var tasksInOldColumn = await _context.Tasks
                .Where(t => t.ColumnId == oldColumnId && t.Id != id)
                .OrderBy(t => t.Position)
                .ToListAsync();

            for (int i = 0; i < tasksInOldColumn.Count; i++)
            {
                tasksInOldColumn[i].Position = i;
            }

            // Insert into new column
            var tasksInNewColumn = await _context.Tasks
                .Where(t => t.ColumnId == newColumnId)
                .OrderBy(t => t.Position)
                .ToListAsync();

            task.ColumnId = newColumnId;
            tasksInNewColumn.Insert(newPosition, task);

            for (int i = 0; i < tasksInNewColumn.Count; i++)
            {
                tasksInNewColumn[i].Position = i;
            }
        }

        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _projectHub.Clients.Group(task.ProjectId.ToString()).SendAsync("TaskMoved", id, newColumnId, newPosition);

        return NoContent();
    }

    // Checklist
    /// <summary>
    /// Adds a checklist item to a task.
    /// </summary>
    /// <param name="id">The unique identifier of the task.</param>
    /// <param name="dto">The checklist item creation data.</param>
    /// <returns>The created checklist item.</returns>
    [HttpPost("tasks/{id}/checklist")]
    public async Task<ActionResult<ChecklistItemDto>> AddChecklistItem(Guid id, CreateChecklistItemDto dto)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        var maxPos = await _context.ChecklistItems
            .Where(c => c.TaskId == id)
            .MaxAsync(c => (int?)c.Position) ?? -1;

        var item = new ChecklistItem
        {
            Id = Guid.NewGuid(),
            TaskId = id,
            Text = dto.Title,
            IsCompleted = false,
            Position = maxPos + 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChecklistItems.Add(item);
        await _context.SaveChangesAsync();

        var itemDto = new ChecklistItemDto
        {
            Id = item.Id,
            Title = item.Text,
            IsCompleted = item.IsCompleted,
            Position = item.Position
        };

        await _projectHub.Clients.Group(task.ProjectId.ToString()).SendAsync("ChecklistItemAdded", id, itemDto);

        return Ok(itemDto);
    }

    /// <summary>
    /// Updates a checklist item.
    /// </summary>
    /// <param name="id">The unique identifier of the task.</param>
    /// <param name="itemId">The unique identifier of the checklist item.</param>
    /// <param name="dto">The checklist item update data.</param>
    /// <returns>The updated checklist item.</returns>
    [HttpPut("tasks/{id}/checklist/{itemId}")]
    public async Task<ActionResult<ChecklistItemDto>> UpdateChecklistItem(Guid id, Guid itemId, UpdateChecklistItemDto dto)
    {
        var item = await _context.ChecklistItems.Include(c => c.Task).FirstOrDefaultAsync(c => c.Id == itemId && c.TaskId == id);
        if (item == null) return NotFound();

        item.Text = dto.Title;
        item.IsCompleted = dto.IsCompleted;
        
        await _context.SaveChangesAsync();

        var itemDto = new ChecklistItemDto
        {
            Id = item.Id,
            Title = item.Text,
            IsCompleted = item.IsCompleted,
            Position = item.Position
        };

        await _projectHub.Clients.Group(item.Task!.ProjectId.ToString()).SendAsync("ChecklistItemUpdated", id, itemDto);

        return Ok(itemDto);
    }

    /// <summary>
    /// Deletes a checklist item.
    /// </summary>
    /// <param name="id">The unique identifier of the task.</param>
    /// <param name="itemId">The unique identifier of the checklist item.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("tasks/{id}/checklist/{itemId}")]
    public async Task<IActionResult> DeleteChecklistItem(Guid id, Guid itemId)
    {
        var item = await _context.ChecklistItems.Include(c => c.Task).FirstOrDefaultAsync(c => c.Id == itemId && c.TaskId == id);
        if (item == null) return NotFound();

        var projectId = item.Task!.ProjectId;
        _context.ChecklistItems.Remove(item);
        await _context.SaveChangesAsync();

        await _projectHub.Clients.Group(projectId.ToString()).SendAsync("ChecklistItemDeleted", id, itemId);

        return NoContent();
    }

    // Comments
    /// <summary>
    /// Adds a comment to a task.
    /// </summary>
    /// <param name="id">The unique identifier of the task.</param>
    /// <param name="dto">The comment creation data.</param>
    /// <returns>The created comment.</returns>
    [HttpPost("tasks/{id}/comments")]
    public async Task<ActionResult<TaskCommentDto>> AddComment(Guid id, CreateCommentDto dto)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = id,
            UserId = userId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.TaskComments.Add(comment);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);

        var commentDto = new TaskCommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UserId = userId,
            UserName = user?.DisplayName ?? "Unknown",
            UserAvatar = user?.AvatarUrl
        };

        await _projectHub.Clients.Group(task.ProjectId.ToString()).SendAsync("CommentAdded", id, commentDto);

        // Notification: Comment on assigned task
        if (task.AssignedToId.HasValue && task.AssignedToId != userId)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = task.AssignedToId.Value,
                Title = "New Comment",
                Message = $"{user?.DisplayName ?? "Someone"} commented on task: {task.Title}",
                Type = "Info",
                RelatedEntityId = task.Id,
                RelatedEntityType = "Task",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                RelatedEntityId = notification.RelatedEntityId,
                RelatedEntityType = notification.RelatedEntityType
            };

            await _projectHub.Clients.User(task.AssignedToId.Value.ToString()).SendAsync("NotificationReceived", notificationDto);
        }

        return Ok(commentDto);
    }

    /// <summary>
    /// Deletes a comment from a task.
    /// </summary>
    /// <param name="id">The unique identifier of the task.</param>
    /// <param name="commentId">The unique identifier of the comment.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("tasks/{id}/comments/{commentId}")]
    public async Task<IActionResult> DeleteComment(Guid id, Guid commentId)
    {
        var comment = await _context.TaskComments.Include(c => c.Task).FirstOrDefaultAsync(c => c.Id == commentId && c.TaskId == id);
        if (comment == null) return NotFound();

        // Check permissions (only author or admin/project owner - for now just author)
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (comment.UserId != userId)
        {
            return Forbid();
        }

        var projectId = comment.Task!.ProjectId;
        _context.TaskComments.Remove(comment);
        await _context.SaveChangesAsync();

        await _projectHub.Clients.Group(projectId.ToString()).SendAsync("CommentDeleted", id, commentId);

        return NoContent();
    }

    // Tags
    /// <summary>
    /// Adds a tag to a task.
    /// </summary>
    /// <param name="id">The unique identifier of the task.</param>
    /// <param name="dto">The tag creation data.</param>
    /// <returns>The created tag.</returns>
    [HttpPost("tasks/{id}/tags")]
    public async Task<ActionResult<TaskTagDto>> AddTag(Guid id, AddTagDto dto)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        var tag = new TaskTag
        {
            Id = Guid.NewGuid(),
            TaskId = id,
            Name = dto.Name,
            Color = dto.Color
        };

        _context.TaskTags.Add(tag);
        await _context.SaveChangesAsync();

        var tagDto = new TaskTagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };

        await _projectHub.Clients.Group(task.ProjectId.ToString()).SendAsync("TagAdded", id, tagDto);

        return Ok(tagDto);
    }

    /// <summary>
    /// Deletes a tag from a task.
    /// </summary>
    /// <param name="id">The unique identifier of the task.</param>
    /// <param name="tagId">The unique identifier of the tag.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("tasks/{id}/tags/{tagId}")]
    public async Task<IActionResult> DeleteTag(Guid id, Guid tagId)
    {
        var tag = await _context.TaskTags.Include(t => t.Task).FirstOrDefaultAsync(t => t.Id == tagId && t.TaskId == id);
        if (tag == null) return NotFound();

        var projectId = tag.Task!.ProjectId;
        _context.TaskTags.Remove(tag);
        await _context.SaveChangesAsync();

        await _projectHub.Clients.Group(projectId.ToString()).SendAsync("TagDeleted", id, tagId);

        return NoContent();
    }

    // Attachments
    /// <summary>
    /// Uploads an attachment to a task.
    /// </summary>
    /// <param name="id">The unique identifier of the task.</param>
    /// <param name="file">The file to upload.</param>
    /// <returns>The created attachment details.</returns>
    [HttpPost("tasks/{id}/attachments")]
    public async Task<ActionResult<TaskAttachmentDto>> UploadAttachment(Guid id, IFormFile file)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Ensure directory exists
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new TaskAttachment
        {
            Id = Guid.NewGuid(),
            TaskId = id,
            FileName = file.FileName,
            FilePath = $"/uploads/{uniqueFileName}",
            FileSize = file.Length,
            ContentType = file.ContentType,
            UploadedById = userId,
            UploadedAt = DateTime.UtcNow
        };

        _context.TaskAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);

        var attachmentDto = new TaskAttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            FilePath = attachment.FilePath,
            UploadedAt = attachment.UploadedAt,
            UploadedById = userId,
            UploadedByName = user?.DisplayName ?? "Unknown"
        };

        await _projectHub.Clients.Group(task.ProjectId.ToString()).SendAsync("AttachmentAdded", id, attachmentDto);

        return Ok(attachmentDto);
    }

    /// <summary>
    /// Deletes an attachment from a task.
    /// </summary>
    /// <param name="id">The unique identifier of the task.</param>
    /// <param name="attachmentId">The unique identifier of the attachment.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("tasks/{id}/attachments/{attachmentId}")]
    public async Task<IActionResult> DeleteAttachment(Guid id, Guid attachmentId)
    {
        var attachment = await _context.TaskAttachments.Include(a => a.Task).FirstOrDefaultAsync(a => a.Id == attachmentId && a.TaskId == id);
        if (attachment == null) return NotFound();

        var projectId = attachment.Task!.ProjectId;
        
        // Delete file from disk
        // Note: FilePath is relative URL, need to map to physical path
        var fileName = Path.GetFileName(attachment.FilePath);
        var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
        
        if (System.IO.File.Exists(physicalPath))
        {
            System.IO.File.Delete(physicalPath);
        }

        _context.TaskAttachments.Remove(attachment);
        await _context.SaveChangesAsync();

        await _projectHub.Clients.Group(projectId.ToString()).SendAsync("AttachmentDeleted", id, attachmentId);

        return NoContent();
    }
}

using UnityDevHub.API.Models.Auth;

namespace UnityDevHub.API.Models.Task;

public class TaskDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid ColumnId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public string? AssignedToAvatar { get; set; }
    public int Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public int Position { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public int CommentsCount { get; set; }
    public int AttachmentsCount { get; set; }
    public int ChecklistTotal { get; set; }
    public int ChecklistCompleted { get; set; }
    
    public int TaskNumber { get; set; }
    public List<TaskTagDto> Tags { get; set; } = new List<TaskTagDto>();
    public List<ChecklistItemDto> ChecklistItems { get; set; } = new List<ChecklistItemDto>();
    public List<TaskCommentDto> Comments { get; set; } = new List<TaskCommentDto>();
    public List<TaskAttachmentDto> Attachments { get; set; } = new List<TaskAttachmentDto>();
    public List<CommitDto> Commits { get; set; } = new List<CommitDto>();
}

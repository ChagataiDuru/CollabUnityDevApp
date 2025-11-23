using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnityDevHub.API.Data.Entities;

[Table("Tasks")]
public class ProjectTask
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }

    public Guid ColumnId { get; set; }
    public TaskColumn? Column { get; set; }

    public Guid? SprintId { get; set; }
    public Sprint? Sprint { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public int TaskNumber { get; set; }

    public string? Description { get; set; }

    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }

    public int Priority { get; set; } = 1; // 1=Low, 2=Medium, 3=High

    public DateTime? DueDate { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? EstimatedHours { get; set; }

    public int Position { get; set; }

    public Guid? CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TaskTag> Tags { get; set; } = new List<TaskTag>();
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
    public ICollection<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();
    public ICollection<Commit> Commits { get; set; } = new List<Commit>();
    public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
}

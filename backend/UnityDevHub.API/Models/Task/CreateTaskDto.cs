using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Models.Task;

public class CreateTaskDto
{
    [Required]
    public Guid ColumnId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public Guid? AssignedToId { get; set; }
    public int Priority { get; set; } = 1;
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
}

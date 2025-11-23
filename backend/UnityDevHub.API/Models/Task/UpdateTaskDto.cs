using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Models.Task;

public class UpdateTaskDto
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public Guid? AssignedToId { get; set; }
    public int Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
}

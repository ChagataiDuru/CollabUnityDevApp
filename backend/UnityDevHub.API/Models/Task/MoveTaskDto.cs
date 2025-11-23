using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Models.Task;

public class MoveTaskDto
{
    [Required]
    public Guid NewColumnId { get; set; }

    [Required]
    public int NewPosition { get; set; }
}

namespace UnityDevHub.API.Models.Task;

public class ChecklistItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int Position { get; set; }
}

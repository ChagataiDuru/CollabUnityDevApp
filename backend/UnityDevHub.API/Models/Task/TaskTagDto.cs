namespace UnityDevHub.API.Models.Task;

public class TaskTagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

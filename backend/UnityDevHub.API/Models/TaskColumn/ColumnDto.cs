namespace UnityDevHub.API.Models.TaskColumn;

public class ColumnDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Position { get; set; }
    public string Color { get; set; } = "#64748b";
}

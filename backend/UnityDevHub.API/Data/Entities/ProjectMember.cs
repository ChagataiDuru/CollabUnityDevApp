namespace UnityDevHub.API.Data.Entities;

public enum ProjectRole
{
    Owner = 0,
    Admin = 1,
    Member = 2,
    Viewer = 3
}

public class ProjectMember
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public ProjectRole Role { get; set; } = ProjectRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Project? Project { get; set; }
    public User? User { get; set; }
}

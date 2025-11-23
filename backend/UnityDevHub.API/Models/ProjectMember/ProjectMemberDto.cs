using UnityDevHub.API.Data.Entities;

namespace UnityDevHub.API.Models.ProjectMember;

public class ProjectMemberDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public ProjectRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}

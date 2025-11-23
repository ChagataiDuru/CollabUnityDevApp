using UnityDevHub.API.Data.Entities;

namespace UnityDevHub.API.Models.ProjectMember;

public class AddMemberDto
{
    public Guid UserId { get; set; }
    public ProjectRole Role { get; set; }
}

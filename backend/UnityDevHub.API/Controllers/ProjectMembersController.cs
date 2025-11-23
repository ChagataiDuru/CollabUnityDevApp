using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UnityDevHub.API.Data;
using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Hubs;
using UnityDevHub.API.Models.Auth;
using UnityDevHub.API.Models.ProjectMember;

namespace UnityDevHub.API.Controllers;

[Authorize]
[ApiController]
[Route("api/projects")]
public class ProjectMembersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<ProjectHub> _hubContext;

    public ProjectMembersController(ApplicationDbContext context, IHubContext<ProjectHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");

    private async Task<ProjectRole?> GetUserRole(Guid projectId, Guid userId)
    {
        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        return member?.Role;
    }

    // GET: api/projects/{projectId}/members
    [HttpGet("{projectId}/members")]
    public async Task<ActionResult<List<ProjectMemberDto>>> GetProjectMembers(Guid projectId)
    {
        var userId = GetUserId();
        var userRole = await GetUserRole(projectId, userId);
        
        if (userRole == null)
        {
            return Forbid();
        }

        var members = await _context.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .Include(pm => pm.User)
            .Select(pm => new ProjectMemberDto
            {
                Id = pm.Id,
                UserId = pm.UserId.ToString(),
                Username = pm.User!.Username,
                DisplayName = pm.User.DisplayName,
                AvatarUrl = pm.User.AvatarUrl,
                Role = pm.Role,
                JoinedAt = pm.JoinedAt
            })
            .OrderBy(pm => pm.JoinedAt)
            .ToListAsync();

        return Ok(members);
    }

    // GET: api/projects/search-users?q={query}
    [HttpGet("search-users")]
    public async Task<ActionResult<List<UserDto>>> SearchUsers([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 3)
        {
            return Ok(new List<UserDto>());
        }

        var users = await _context.Users
            .Where(u => u.Username.Contains(q) || (u.DisplayName != null && u.DisplayName.Contains(q)))
            .Take(10)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                DisplayName = u.DisplayName,
                AvatarUrl = u.AvatarUrl
            })
            .ToListAsync();

        return Ok(users);
    }

    // POST: api/projects/{projectId}/members
    [HttpPost("{projectId}/members")]
    public async Task<ActionResult<ProjectMemberDto>> AddMember(Guid projectId, [FromBody] AddMemberDto dto)
    {
        var currentUserId = GetUserId();
        var currentUserRole = await GetUserRole(projectId, currentUserId);

        // Only Owners and Admins can add members
        if (currentUserRole != ProjectRole.Owner && currentUserRole != ProjectRole.Admin)
        {
            return Forbid();
        }

        // Check if user exists
        var userToAdd = await _context.Users.FindAsync(dto.UserId);
        if (userToAdd == null)
        {
            return NotFound("User not found");
        }

        // Check if already a member
        var existingMember = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == dto.UserId);
        
        if (existingMember != null)
        {
            return BadRequest("User is already a member of this project");
        }

        var member = new ProjectMember
        {
            ProjectId = projectId,
            UserId = dto.UserId,
            Role = dto.Role
        };

        _context.ProjectMembers.Add(member);
        await _context.SaveChangesAsync();

        // Notify via SignalR
        await _hubContext.Clients.Group(projectId.ToString()).SendAsync("MemberJoined", new
        {
            userId = dto.UserId.ToString(),
            username = userToAdd.Username,
            role = dto.Role
        });

        return Ok(new ProjectMemberDto
        {
            Id = member.Id,
            UserId = member.UserId.ToString(),
            Username = userToAdd.Username,
            DisplayName = userToAdd.DisplayName,
            AvatarUrl = userToAdd.AvatarUrl,
            Role = member.Role,
            JoinedAt = member.JoinedAt
        });
    }

    // DELETE: api/projects/{projectId}/members/{userId}
    [HttpDelete("{projectId}/members/{memberId}")]
    public async Task<ActionResult> RemoveMember(Guid projectId, Guid memberId)
    {
        var currentUserId = GetUserId();
        var currentUserRole = await GetUserRole(projectId, currentUserId);

        // Only Owners and Admins can remove members
        if (currentUserRole != ProjectRole.Owner && currentUserRole != ProjectRole.Admin)
        {
            return Forbid();
        }

        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == memberId);

        if (member == null)
        {
            return NotFound("Member not found");
        }

        // Cannot remove owner or yourself
        if (member.Role == ProjectRole.Owner)
        {
            return BadRequest("Cannot remove project owner");
        }

        if (memberId == currentUserId)
        {
            return BadRequest("Cannot remove yourself from the project");
        }

        _context.ProjectMembers.Remove(member);
        await _context.SaveChangesAsync();

        // Notify via SignalR
        await _hubContext.Clients.Group(projectId.ToString()).SendAsync("MemberRemoved", new
        {
            userId = memberId.ToString()
        });

        return NoContent();
    }

    // PUT: api/projects/{projectId}/members/{userId}/role
    [HttpPut("{projectId}/members/{memberId}/role")]
    public async Task<ActionResult> UpdateMemberRole(Guid projectId, Guid memberId, [FromBody] ProjectRole newRole)
    {
        var currentUserId = GetUserId();
        var currentUserRole = await GetUserRole(projectId, currentUserId);

        // Only Owners can update roles
        if (currentUserRole != ProjectRole.Owner)
        {
            return Forbid();
        }

        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == memberId);

        if (member == null)
        {
            return NotFound("Member not found");
        }

        // Cannot change owner role
        if (member.Role == ProjectRole.Owner || newRole == ProjectRole.Owner)
        {
            return BadRequest("Cannot change owner role");
        }

        member.Role = newRole;
        await _context.SaveChangesAsync();

        // Notify via SignalR
        await _hubContext.Clients.Group(projectId.ToString()).SendAsync("MemberRoleUpdated", new
        {
            userId = memberId.ToString(),
            role = newRole
        });

        return Ok(new { message = "Role updated successfully" });
    }
}

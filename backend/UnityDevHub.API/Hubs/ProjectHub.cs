using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UnityDevHub.API.Data.Entities;

namespace UnityDevHub.API.Hubs;

[Authorize]
public class ProjectHub : Hub
{
    public async Task JoinProject(string projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, projectId);
    }

    public async Task LeaveProject(string projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, projectId);
    }

    // Methods called by clients or backend to broadcast updates
    // These are usually invoked from Controllers/Services via IHubContext<ProjectHub>
}

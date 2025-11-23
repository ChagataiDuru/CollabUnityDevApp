using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UnityDevHub.API.Models.Whiteboard;

namespace UnityDevHub.API.Hubs;

[Authorize]
public class WhiteboardHub : Hub
{
    public async Task JoinWhiteboard(string projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, projectId);
    }

    public async Task LeaveWhiteboard(string projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, projectId);
    }

    public async Task SendDraw(string projectId, DrawEventDto drawEvent)
    {
        await Clients.OthersInGroup(projectId).SendAsync("ReceiveDraw", drawEvent);
    }

    public async Task ClearBoard(string projectId)
    {
        await Clients.OthersInGroup(projectId).SendAsync("BoardCleared");
    }
}

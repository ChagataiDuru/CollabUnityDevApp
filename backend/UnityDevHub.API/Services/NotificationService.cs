using Microsoft.AspNetCore.SignalR;
using UnityDevHub.API.Data;
using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Hubs;
using UnityDevHub.API.Models.Notification;

namespace UnityDevHub.API.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<ProjectHub> _hubContext;

    public NotificationService(ApplicationDbContext context, IHubContext<ProjectHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(Guid userId, string title, string message, string type, Guid relatedEntityId, string relatedEntityType)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var notificationDto = new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            RelatedEntityId = notification.RelatedEntityId,
            RelatedEntityType = notification.RelatedEntityType
        };

        await _hubContext.Clients.User(userId.ToString()).SendAsync("NotificationReceived", notificationDto);
    }
}

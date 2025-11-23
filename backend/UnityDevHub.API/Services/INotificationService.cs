using UnityDevHub.API.Models.Notification;

namespace UnityDevHub.API.Services;

public interface INotificationService
{
    Task SendNotificationAsync(Guid userId, string title, string message, string type, Guid relatedEntityId, string relatedEntityType);
}

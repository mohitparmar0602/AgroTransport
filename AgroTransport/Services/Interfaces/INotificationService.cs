using System.Collections.Generic;
using System.Threading.Tasks;
using AgroTransport.Models;

namespace AgroTransport.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string title, string message, string type, int? relatedEntityId = null, string? linkUrl = null);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int limit = 20);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
    }
}


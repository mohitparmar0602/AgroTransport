using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgroTransport.Data;
using AgroTransport.Models;
using AgroTransport.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgroTransport.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(string userId, string title, string message, string type, int? relatedEntityId = null, string? linkUrl = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                NotificationType = type,
                RelatedEntityId = relatedEntityId,
                LinkUrl = linkUrl,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int limit = 20)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unread.Any())
            {
                foreach (var item in unread)
                {
                    item.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}


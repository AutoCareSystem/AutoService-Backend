using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Notification_Service.Data;
using Notification_Service.DTOs;
using Notification_Service.Hubs;
using Notification_Service.Models;

namespace Notification_Service.Services;

public class NotificationService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        AppDbContext context,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<Notification> CreateNotificationAsync(CreateNotificationDto dto)
    {
        var notification = new Notification
        {
            UserID = dto.UserID,
            NotificationType = dto.NotificationType,
            Title = dto.Title,
            Message = dto.Message,
            EntityType = dto.EntityType,
            EntityID = dto.EntityID,
            Metadata = dto.Metadata,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Send real-time notification via SignalR
        await SendRealtimeNotification(notification);

        _logger.LogInformation($"✅ Notification created for user {dto.UserID}: {dto.Title}");

        return notification;
    }

    public async Task SendRealtimeNotification(Notification notification)
    {
        try
        {
            var notificationDto = new NotificationDto
            {
                NotificationID = notification.NotificationID,
                UserID = notification.UserID,
                NotificationType = notification.NotificationType,
                Title = notification.Title,
                Message = notification.Message,
                EntityType = notification.EntityType,
                EntityID = notification.EntityID,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                Metadata = notification.Metadata
            };

            // Send to specific user group
            await _hubContext.Clients.Group(notification.UserID)
                .SendAsync("ReceiveNotification", notificationDto);

            _logger.LogInformation($"📤 Real-time notification sent to user {notification.UserID}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error sending real-time notification: {ex.Message}");
        }
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, bool unreadOnly = false, int limit = 50)
    {
        var query = _context.Notifications
            .Where(n => n.UserID == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .Select(n => new NotificationDto
            {
                NotificationID = n.NotificationID,
                UserID = n.UserID,
                NotificationType = n.NotificationType,
                Title = n.Title,
                Message = n.Message,
                EntityType = n.EntityType,
                EntityID = n.EntityID,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt,
                Metadata = n.Metadata
            })
            .ToListAsync();

        return notifications;
    }

    public async Task<NotificationSummaryDto> GetNotificationSummaryAsync(string userId)
    {
        var totalCount = await _context.Notifications
            .CountAsync(n => n.UserID == userId);

        var unreadCount = await _context.Notifications
            .CountAsync(n => n.UserID == userId && !n.IsRead);

        var recentNotifications = await GetUserNotificationsAsync(userId, false, 10);

        return new NotificationSummaryDto
        {
            TotalCount = totalCount,
            UnreadCount = unreadCount,
            RecentNotifications = recentNotifications
        };
    }

    public async Task<bool> MarkAsReadAsync(string userId, List<int> notificationIds)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserID == userId && notificationIds.Contains(n.NotificationID) && !n.IsRead)
            .ToListAsync();

        if (!notifications.Any())
            return false;

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Send real-time update
        await _hubContext.Clients.Group(userId)
            .SendAsync("NotificationsMarkedAsRead", notificationIds);

        _logger.LogInformation($"✅ Marked {notifications.Count} notifications as read for user {userId}");

        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserID == userId && !n.IsRead)
            .ToListAsync();

        if (!notifications.Any())
            return false;

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Send real-time update
        await _hubContext.Clients.Group(userId)
            .SendAsync("AllNotificationsMarkedAsRead");

        _logger.LogInformation($"✅ Marked all notifications as read for user {userId}");

        return true;
    }

    public async Task<bool> DeleteNotificationAsync(string userId, int notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationID == notificationId && n.UserID == userId);

        if (notification == null)
            return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"🗑️ Deleted notification {notificationId} for user {userId}");

        return true;
    }
}

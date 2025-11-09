namespace Notification_Service.DTOs;

public class NotificationDto
{
    public int NotificationID { get; set; }
    public string UserID { get; set; } = null!;
    public string NotificationType { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? EntityType { get; set; }
    public int? EntityID { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? Metadata { get; set; }
}

public class CreateNotificationDto
{
    public string UserID { get; set; } = null!;
    public string NotificationType { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? EntityType { get; set; }
    public int? EntityID { get; set; }
    public string? Metadata { get; set; }
}

public class MarkAsReadDto
{
    public List<int> NotificationIDs { get; set; } = new();
}

public class NotificationSummaryDto
{
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public List<NotificationDto> RecentNotifications { get; set; } = new();
}

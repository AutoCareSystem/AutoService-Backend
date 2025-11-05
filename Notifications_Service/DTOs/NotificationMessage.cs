using System.ComponentModel.DataAnnotations;

namespace Notifications_Service.DTOs;

public class NotificationMessage
{
    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;

    public string? Type { get; set; } // "appointment", "profile", "status_update"

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public object? Data { get; set; } // Additional data (appointment details, etc.)
}

public class AppointmentNotificationData
{
    public int AppointmentId { get; set; }
    public string? CustomerName { get; set; }
    public string? EmployeeName { get; set; }
    public string? ServiceTitle { get; set; }
    public DateTime? Date { get; set; }
    public string? Status { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notification_Service.Models;

[Table("Notifications")]
public class Notification
{
    [Key]
    public int NotificationID { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserID { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string NotificationType { get; set; } = null!; // Appointment, Service, Project

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = null!;

    [MaxLength(50)]
    public string? EntityType { get; set; } // Appointment, Service, Project

    public int? EntityID { get; set; } // ID of the related entity

    [Required]
    public bool IsRead { get; set; } = false;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }

    [MaxLength(1000)]
    public string? Metadata { get; set; } // JSON string for additional data
}

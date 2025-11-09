using Microsoft.AspNetCore.Mvc;
using Notification_Service.DTOs;
using Notification_Service.Services;

namespace Notification_Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        NotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all notifications for a user
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<NotificationDto>>> GetUserNotifications(
        string userId,
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int limit = 50)
    {
        try
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly, limit);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting notifications for user {userId}: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while fetching notifications" });
        }
    }

    /// <summary>
    /// Get notification summary (total, unread count, recent notifications)
    /// </summary>
    [HttpGet("user/{userId}/summary")]
    public async Task<ActionResult<NotificationSummaryDto>> GetNotificationSummary(string userId)
    {
        try
        {
            var summary = await _notificationService.GetNotificationSummaryAsync(userId);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting notification summary for user {userId}: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while fetching notification summary" });
        }
    }

    /// <summary>
    /// Create a manual notification (for testing or admin purposes)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var notification = await _notificationService.CreateNotificationAsync(dto);

            return CreatedAtAction(
                nameof(GetUserNotifications),
                new { userId = notification.UserID },
                new NotificationDto
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
                });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating notification: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while creating notification" });
        }
    }

    /// <summary>
    /// Mark specific notifications as read
    /// </summary>
    [HttpPut("user/{userId}/mark-read")]
    public async Task<IActionResult> MarkNotificationsAsRead(
        string userId,
        [FromBody] MarkAsReadDto dto)
    {
        try
        {
            if (!dto.NotificationIDs.Any())
                return BadRequest(new { message = "No notification IDs provided" });

            var success = await _notificationService.MarkAsReadAsync(userId, dto.NotificationIDs);

            if (!success)
                return NotFound(new { message = "No unread notifications found with the provided IDs" });

            return Ok(new { message = $"Marked {dto.NotificationIDs.Count} notification(s) as read" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error marking notifications as read for user {userId}: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while updating notifications" });
        }
    }

    /// <summary>
    /// Mark all notifications as read for a user
    /// </summary>
    [HttpPut("user/{userId}/mark-all-read")]
    public async Task<IActionResult> MarkAllNotificationsAsRead(string userId)
    {
        try
        {
            var success = await _notificationService.MarkAllAsReadAsync(userId);

            if (!success)
                return NotFound(new { message = "No unread notifications found" });

            return Ok(new { message = "All notifications marked as read" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error marking all notifications as read for user {userId}: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while updating notifications" });
        }
    }

    /// <summary>
    /// Delete a specific notification
    /// </summary>
    [HttpDelete("user/{userId}/{notificationId}")]
    public async Task<IActionResult> DeleteNotification(string userId, int notificationId)
    {
        try
        {
            var success = await _notificationService.DeleteNotificationAsync(userId, notificationId);

            if (!success)
                return NotFound(new { message = "Notification not found" });

            return Ok(new { message = "Notification deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting notification {notificationId} for user {userId}: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while deleting notification" });
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            service = "Notification Service",
            timestamp = DateTime.UtcNow
        });
    }
}

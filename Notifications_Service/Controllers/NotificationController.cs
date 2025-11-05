using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Notifications_Service.Hubs;
using Notifications_Service.DTOs;

namespace Notifications_Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationController> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    // POST: api/Notification/send
    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] NotificationMessage message)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Send notification to specific user group
            await _hubContext.Clients.Group($"user_{message.UserId}")
                .SendAsync("ReceiveNotification", new
                {
                    content = message.Content,
                    type = message.Type,
                    timestamp = message.Timestamp,
                    data = message.Data
                });

            _logger.LogInformation($"✅ Notification sent to user {message.UserId}: {message.Content}");

            return Ok(new
            {
                success = true,
                message = "Notification sent successfully",
                userId = message.UserId,
                timestamp = message.Timestamp
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error sending notification: {ex.Message}");
            return StatusCode(500, new { error = "Failed to send notification" });
        }
    }

    // POST: api/Notification/appointment-created
    [HttpPost("appointment-created")]
    public async Task<IActionResult> NotifyAppointmentCreated([FromBody] NotificationMessage message)
    {
        try
        {
            var appointmentData = message.Data as AppointmentNotificationData;

            var notificationContent = new
            {
                content = $"🔔 New Appointment Created by {appointmentData?.CustomerName}",
                type = "appointment_created",
                timestamp = message.Timestamp,
                data = message.Data
            };

            await _hubContext.Clients.Group($"user_{message.UserId}")
                .SendAsync("ReceiveNotification", notificationContent);

            _logger.LogInformation($"✅ Appointment creation notification sent to Employee {message.UserId}");

            return Ok(new { success = true, message = "Employee notified" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error: {ex.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // POST: api/Notification/appointment-status-updated
    [HttpPost("appointment-status-updated")]
    public async Task<IActionResult> NotifyAppointmentStatusUpdated([FromBody] NotificationMessage message)
    {
        try
        {
            var appointmentData = message.Data as AppointmentNotificationData;

            var notificationContent = new
            {
                content = $"✅ Your appointment has been {appointmentData?.Status}",
                type = "appointment_status_updated",
                timestamp = message.Timestamp,
                data = message.Data
            };

            await _hubContext.Clients.Group($"user_{message.UserId}")
                .SendAsync("ReceiveNotification", notificationContent);

            _logger.LogInformation($"✅ Appointment status notification sent to Customer {message.UserId}");

            return Ok(new { success = true, message = "Customer notified" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error: {ex.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // POST: api/Notification/profile-updated
    [HttpPost("profile-updated")]
    public async Task<IActionResult> NotifyProfileUpdated([FromBody] NotificationMessage message)
    {
        try
        {
            var notificationContent = new
            {
                content = "✅ Your profile was updated successfully",
                type = "profile_updated",
                timestamp = message.Timestamp,
                data = message.Data
            };

            await _hubContext.Clients.Group($"user_{message.UserId}")
                .SendAsync("ReceiveNotification", notificationContent);

            _logger.LogInformation($"✅ Profile update notification sent to user {message.UserId}");

            return Ok(new { success = true, message = "User notified" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error: {ex.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // POST: api/Notification/broadcast (Admin only)
    [HttpPost("broadcast")]
    public async Task<IActionResult> BroadcastNotification([FromBody] NotificationMessage message)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
            {
                content = message.Content,
                type = "broadcast",
                timestamp = message.Timestamp
            });

            _logger.LogInformation($"✅ Broadcast notification sent to all users");

            return Ok(new { success = true, message = "Broadcast sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error: {ex.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // GET: api/Notification/health
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            service = "Notifications_Service",
            status = "running",
            timestamp = DateTime.UtcNow
        });
    }
}

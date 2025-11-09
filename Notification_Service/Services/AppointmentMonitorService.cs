using Microsoft.EntityFrameworkCore;
using Notification_Service.Data;
using Notification_Service.DTOs;

namespace Notification_Service.Services;

public class AppointmentMonitorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AppointmentMonitorService> _logger;
    private readonly Dictionary<int, string> _lastKnownStatus = new();

    public AppointmentMonitorService(
        IServiceProvider serviceProvider,
        ILogger<AppointmentMonitorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Appointment Monitor Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MonitorAppointmentsAsync();
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Check every 10 seconds
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in Appointment Monitor: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Wait longer on error
            }
        }
    }

    private async Task MonitorAppointmentsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

        var appointments = await context.Appointments
            .Where(a => a.Status != "Cancelled" && a.Status != "Rejected")
            .ToListAsync();

        foreach (var appointment in appointments)
        {
            // Check if status changed
            if (_lastKnownStatus.TryGetValue(appointment.AppointmentID, out var lastStatus))
            {
                if (lastStatus != appointment.Status)
                {
                    await HandleStatusChange(appointment, lastStatus, notificationService);
                    _lastKnownStatus[appointment.AppointmentID] = appointment.Status;
                }
            }
            else
            {
                // New appointment detected
                _lastKnownStatus[appointment.AppointmentID] = appointment.Status;
                await HandleNewAppointment(appointment, notificationService);
            }
        }
    }

    private async Task HandleNewAppointment(Models.Appointment appointment, NotificationService notificationService)
    {
        // Notify customer about new appointment
        await notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            UserID = appointment.CustomerID,
            NotificationType = "Appointment",
            Title = "New Appointment Created",
            Message = $"Your {appointment.AppointmentType} appointment has been created for {appointment.StartDate.ToShortDateString()} at {appointment.Time}.",
            EntityType = "Appointment",
            EntityID = appointment.AppointmentID,
            Metadata = $"{{\"status\":\"{appointment.Status}\",\"type\":\"{appointment.AppointmentType}\"}}"
        });

        _logger.LogInformation($"📝 New appointment detected: {appointment.AppointmentID}");
    }

    private async Task HandleStatusChange(Models.Appointment appointment, string oldStatus, NotificationService notificationService)
    {
        _logger.LogInformation($"🔄 Appointment {appointment.AppointmentID} status changed: {oldStatus} → {appointment.Status}");

        // Notify customer
        await NotifyCustomer(appointment, oldStatus, notificationService);

        // Notify employee if assigned and status is relevant
        if (!string.IsNullOrEmpty(appointment.EmployeeID) && appointment.Status != "Pending")
        {
            await NotifyEmployee(appointment, oldStatus, notificationService);
        }
    }

    private async Task NotifyCustomer(Models.Appointment appointment, string oldStatus, NotificationService notificationService)
    {
        var message = appointment.Status switch
        {
            "Approved" => $"Great news! Your {appointment.AppointmentType} appointment for {appointment.StartDate.ToShortDateString()} has been accepted and assigned to a technician.",
            "Completed" => $"Your {appointment.AppointmentType} appointment has been completed successfully. Thank you for choosing our service!",
            "Cancelled" => $"Your {appointment.AppointmentType} appointment for {appointment.StartDate.ToShortDateString()} has been cancelled.",
            "Rejected" => $"Unfortunately, your {appointment.AppointmentType} appointment request has been declined. Please contact us for more information.",
            _ => $"Your {appointment.AppointmentType} appointment status has been updated to {appointment.Status}."
        };

        await notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            UserID = appointment.CustomerID,
            NotificationType = "Appointment",
            Title = $"Appointment {appointment.Status}",
            Message = message,
            EntityType = "Appointment",
            EntityID = appointment.AppointmentID,
            Metadata = $"{{\"oldStatus\":\"{oldStatus}\",\"newStatus\":\"{appointment.Status}\",\"type\":\"{appointment.AppointmentType}\"}}"
        });
    }

    private async Task NotifyEmployee(Models.Appointment appointment, string oldStatus, NotificationService notificationService)
    {
        var message = appointment.Status switch
        {
            "Approved" => $"You have been assigned to a {appointment.AppointmentType} appointment on {appointment.StartDate.ToShortDateString()} at {appointment.Time}.",
            "Completed" => $"The {appointment.AppointmentType} appointment you were working on has been marked as completed.",
            _ => $"Appointment status updated to {appointment.Status}."
        };

        await notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            UserID = appointment.EmployeeID!,
            NotificationType = "Appointment",
            Title = $"Appointment Assignment - {appointment.Status}",
            Message = message,
            EntityType = "Appointment",
            EntityID = appointment.AppointmentID,
            Metadata = $"{{\"oldStatus\":\"{oldStatus}\",\"newStatus\":\"{appointment.Status}\",\"type\":\"{appointment.AppointmentType}\"}}"
        });
    }
}

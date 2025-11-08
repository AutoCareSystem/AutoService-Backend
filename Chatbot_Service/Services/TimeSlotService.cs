using Microsoft.EntityFrameworkCore;
using Chatbot_Service.Data;
using Chatbot_Service.DTOs;
using System.Text;

namespace Chatbot_Service.Services;

public class TimeSlotService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TimeSlotService> _logger;

    // Business hours: 9 AM to 6 PM
    private readonly TimeSpan _startTime = new TimeSpan(9, 0, 0);
    private readonly TimeSpan _endTime = new TimeSpan(18, 0, 0);
    private readonly int _slotDurationMinutes = 60; // 1 hour slots

    public TimeSlotService(AppDbContext context, ILogger<TimeSlotService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<AvailableTimeSlotsDto>> GetAvailableTimeSlots(DateTime? preferredDate = null, int numberOfDays = 7)
    {
        try
        {
            // Use UTC to avoid PostgreSQL timezone issues
            var startDate = preferredDate.HasValue 
                ? DateTime.SpecifyKind(preferredDate.Value.Date, DateTimeKind.Utc)
                : DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
            var endDate = startDate.AddDays(numberOfDays);

            // Get all booked appointments in the date range
            var bookedAppointments = await _context.Appointments
                .Where(a => a.StartDate >= startDate && a.StartDate < endDate)
                .Where(a => a.Status != "Cancelled" && a.Status != "Rejected")
                .Select(a => new BookedSlot { StartDate = a.StartDate, Time = a.Time })
                .ToListAsync();

            var availableSlots = new List<AvailableTimeSlotsDto>();

            for (int i = 0; i < numberOfDays; i++)
            {
                var currentDate = startDate.AddDays(i);

                // Skip Sundays (or any day you want to skip)
                if (currentDate.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                var daySlots = GenerateTimeSlotsForDay(currentDate, bookedAppointments);

                if (daySlots.AvailableSlots.Any())
                {
                    availableSlots.Add(daySlots);
                }
            }

            return availableSlots;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting available time slots: {ex.Message}");
            throw;
        }
    }

    private AvailableTimeSlotsDto GenerateTimeSlotsForDay(DateTime date, List<BookedSlot> bookedAppointments)
    {
        var slots = new AvailableTimeSlotsDto
        {
            Date = date,
            AvailableSlots = new List<string>()
        };

        var currentTime = _startTime;

        while (currentTime < _endTime)
        {
            // Check if this slot is booked
            var isBooked = bookedAppointments.Any(a =>
                a.StartDate.Date == date.Date &&
                a.Time == currentTime);

            if (!isBooked)
            {
                // Convert TimeSpan to DateTime to format with AM/PM
                var timeFormatted = DateTime.Today.Add(currentTime).ToString("hh:mm tt");
                slots.AvailableSlots.Add(timeFormatted);
            }

            currentTime = currentTime.Add(TimeSpan.FromMinutes(_slotDurationMinutes));
        }

        return slots;
    }

    // Helper class for booked slots
    private class BookedSlot
    {
        public DateTime StartDate { get; set; }
        public TimeSpan Time { get; set; }
    }

    public async Task<string> GetAvailableSlotsContext(DateTime? preferredDate = null)
    {
        var availableSlots = await GetAvailableTimeSlots(preferredDate, 7);

        if (!availableSlots.Any())
        {
            return "No available time slots found in the next 7 days.";
        }

        var context = new StringBuilder();
        context.AppendLine("Available Time Slots:");

        foreach (var daySlot in availableSlots)
        {
            context.AppendLine($"\n{daySlot.Date:dddd, MMMM dd, yyyy}:");
            context.AppendLine(string.Join(", ", daySlot.AvailableSlots));
        }

        return context.ToString();
    }
}

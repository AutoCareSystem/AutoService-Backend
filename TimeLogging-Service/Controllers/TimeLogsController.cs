using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeLogging_Service.Models;
using TimeLogging_Service.Data;


namespace TimeLogging_ServiceControllers;

[ApiController]
[Route("api/appointments/{appointmentId}/timelogs")]
public class TimeLogsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TimeLogsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start(int appointmentId, [FromBody] StartDto dto)
    {
        var timeLog = new TimeLog
        {
            AppointmentID = appointmentId,
            EmployeeID = dto.EmployeeID,
            StartedAt = DateTimeOffset.UtcNow,
            Notes = dto.Notes
        };

        _db.TimeLogs.Add(timeLog);
        await _db.SaveChangesAsync();

        return Ok(timeLog);
    }

    [HttpPost("stop")]
    public async Task<IActionResult> Stop(int appointmentId, [FromBody] StopDto dto)
    {
        var log = await _db.TimeLogs
            .Where(t => t.AppointmentID == appointmentId && t.EmployeeID == dto.EmployeeID && t.EndedAt == null)
            .OrderByDescending(t => t.StartedAt)
            .FirstOrDefaultAsync();

        if (log == null) return NotFound("No active time log found.");

        log.EndedAt = DateTimeOffset.UtcNow;
        log.DurationMinutes = (log.EndedAt - log.StartedAt)?.TotalMinutes;
        log.Notes = dto.Notes ?? log.Notes;

        await _db.SaveChangesAsync();

        return Ok(log);
    }

    [HttpGet("total")]
    public async Task<IActionResult> GetTotal(int appointmentId)
    {
        var totalMinutes = await _db.TimeLogs
            .Where(t => t.AppointmentID == appointmentId && t.DurationMinutes != null)
            .SumAsync(t => t.DurationMinutes) ?? 0;

        return Ok(new { AppointmentID = appointmentId, TotalMinutes = totalMinutes });
    }

    public class StartDto
    {
        public int EmployeeID { get; set; }
        public string? Notes { get; set; }
    }

    public class StopDto
    {
        public int EmployeeID { get; set; }
        public string? Notes { get; set; }
    }
}

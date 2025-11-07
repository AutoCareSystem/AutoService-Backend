// Controllers/ProjectsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Management_Service.Data;
using Service_Management_Service.Models;
using Service_Management_Service.DTOs;

namespace Service_Management_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProjectsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/projects
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAllProjects()
    {
        var projects = await _context.Appointments
            .Where(a => a.AppointmentType == "Project")
            .Include(a => a.Customer).ThenInclude(c => c.User)
            .Include(a => a.Vehicle)
            .Include(a => a.Employee).ThenInclude(e => e.User)
            .Include(a => a.ProjectDetails)
            .OrderByDescending(a => a.StartDate)
            .ThenBy(a => a.Time)
            .ToListAsync();

        return Ok(projects);
    }

    // GET: api/projects/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Appointment>> GetProjectById(int id)
    {
        var project = await _context.Appointments
            .Where(a => a.AppointmentType == "Project" && a.AppointmentID == id)
            .Include(a => a.Customer).ThenInclude(c => c.User)
            .Include(a => a.Vehicle)
            .Include(a => a.Employee).ThenInclude(e => e.User)
            .Include(a => a.ProjectDetails)
            .FirstOrDefaultAsync();

        if (project == null)
            return NotFound($"Project appointment with ID {id} not found.");

        return Ok(project);
    }

    // PUT: api/projects/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != dto.AppointmentID)
            return BadRequest("ID in URL must match ID in body.");

        var project = await _context.Appointments
            .Include(a => a.ProjectDetails)
            .FirstOrDefaultAsync(a => a.AppointmentType == "Project" && a.AppointmentID == id);

        if (project == null)
            return NotFound($"Project appointment with ID {id} not found.");

        if (project.ProjectDetails == null)
            return BadRequest("Project details missing. This should not happen.");

        // === Update shared fields ===
        project.StartDate = dto.StartDate.Date;
        project.Time = dto.Time;
        project.EndDate = dto.EndDate;
        project.Status = dto.Status ?? project.Status;

        // === Update project-specific fields ===
        project.ProjectDetails.ProjectTitle = dto.ProjectTitle;
        project.ProjectDetails.ProjectDescription = dto.ProjectDescription;

        // === Optional: Update assigned employee (string GUID) ===
        if (!string.IsNullOrWhiteSpace(dto.EmployeeID))
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserID == dto.EmployeeID && e.IsActive);

            if (employee == null)
                return BadRequest("Assigned employee not found or inactive.");

            project.EmployeeID = dto.EmployeeID;
        }

        try
        {
            await _context.SaveChangesAsync();
            return Ok(project);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("The project was modified by another user. Please refresh and try again.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // DELETE: api/projects/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var project = await _context.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentType == "Project" && a.AppointmentID == id);

        if (project == null)
            return NotFound($"Project appointment with ID {id} not found.");

        _context.Appointments.Remove(project);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
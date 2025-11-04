using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Management_Service.Data;
using Service_Management_Service.Models;

namespace Service_Management_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAllProjects()
        {
            var projects = await _context.Appointments
                .Where(a => a.AppointmentType == "Project")
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .ToListAsync();

            return Ok(projects);
        }

        // ✅ GET: api/projects/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetProjectById(int id)
        {
            var project = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .FirstOrDefaultAsync(a => a.AppointmentType == "Project" && a.AppointmentID == id);

            if (project == null)
                return NotFound($"Project appointment with ID {id} not found.");

            return Ok(project);
        }

        // ✅ PUT: api/projects/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] Appointment updatedProject)
        {
            if (id != updatedProject.AppointmentID)
                return BadRequest("ID mismatch.");

            var project = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentType == "Project" && a.AppointmentID == id);

            if (project == null)
                return NotFound($"Project appointment with ID {id} not found.");

            // Update relevant fields
            project.ProjectTitle = updatedProject.ProjectTitle;
            project.ProjectDescription = updatedProject.ProjectDescription;

            project.StartDate = updatedProject.StartDate;
            project.EndDate = updatedProject.EndDate;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(project);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // ✅ DELETE: api/projects/{id}
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
}

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


    }
}

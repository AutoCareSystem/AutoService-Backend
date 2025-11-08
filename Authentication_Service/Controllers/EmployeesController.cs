using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoServiceBackend.Data;
using backend_EAD.DTOs;

namespace backend_EAD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EmployeesController(AppDbContext db)
        {
            _db = db;
        }

        // ======================================================
        // GET: api/Employees
        // ======================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDTO>>> GetAllEmployees()
        {
            var employees = await _db.Employees
                .Include(e => e.User)
                .Select(e => new EmployeeDTO
                {
                    UserID = e.UserID,
                    UserName = e.User.UserName,
                    Email = e.User.Email,
                    PhoneNumber = e.User.PhoneNumber,
                    Role = "Employee",
                    CreatedAt = e.User.CreatedAt,
                    Position = e.Position,
                   
                })
                .ToListAsync();

            return Ok(employees);
        }

        // ======================================================
        // GET: api/Employees/{id}
        // ======================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDTO>> GetEmployeeById(string id)
        {
            var employee = await _db.Employees
                .Include(e => e.User)
                .Where(e => e.UserID == id)
                .Select(e => new EmployeeDTO
                {
                    UserID = e.UserID,
                    UserName = e.User.UserName,
                    Email = e.User.Email,
                    PhoneNumber = e.User.PhoneNumber,
                    Role = "Employee",
                    CreatedAt = e.User.CreatedAt,
                    Position = e.Position,
          
                })
                .FirstOrDefaultAsync();

            if (employee == null)
                return NotFound(new { message = $"Employee with ID '{id}' not found." });

            return Ok(employee);
        }

        // ======================================================
        // PUT: api/Employees/{id}
        // ======================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(string id, [FromBody] UpdateEmployeeDTO dto)
        {
            var employee = await _db.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserID == id);

            if (employee == null)
                return NotFound(new { message = $"Employee with ID '{id}' not found." });

            // Update Identity user details
            if (!string.IsNullOrWhiteSpace(dto.UserName))
                employee.User.UserName = dto.UserName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                employee.User.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                employee.User.PhoneNumber = dto.PhoneNumber;

            // Update Employee-specific fields
            employee.Position = dto.Position;
            

            await _db.SaveChangesAsync();

            return Ok(new { message = "Employee updated successfully." });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoServiceBackend.Data;
using backend_EAD.DTOs;
using backend_EAD.Models;
using Microsoft.AspNetCore.Identity;

namespace backend_EAD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public EmployeesController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
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
                    EmpNo = e.EmpNo,
                    IsActive = e.IsActive
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
                    EmpNo = e.EmpNo,
                    IsActive = e.IsActive
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
            employee.EmpNo = dto.EmpNo;
            employee.IsActive = dto.IsActive;


            await _db.SaveChangesAsync();

            return Ok(new { message = "Employee updated successfully." });
        }

        // ======================================================
        // POST: api/Employees
        // ======================================================
        [HttpPost]
        public async Task<ActionResult<EmployeeDTO>> CreateEmployee([FromBody] CreateEmployeeDTO dto)
        {
            // Create the AppUser
            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Role = "Employee",
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "User creation failed", errors = result.Errors });
            }

            // Create the Employee record
            var employee = new Employee
            {
                UserID = user.Id,
                Position = dto.Position,
                EmpNo = dto.EmpNo,
                IsActive = dto.IsActive
            };

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();

            var employeeDto = new EmployeeDTO
            {
                UserID = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Role = "Employee",
                CreatedAt = user.CreatedAt,
                Position = employee.Position,
                EmpNo = employee.EmpNo,
                IsActive = employee.IsActive
            };

            return CreatedAtAction(nameof(GetEmployeeById), new { id = user.Id }, employeeDto);
        }

        // ======================================================
        // DELETE: api/Employees/{id}
        // ======================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            var employee = await _db.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserID == id);

            if (employee == null)
                return NotFound(new { message = $"Employee with ID '{id}' not found." });

            // Delete the Employee record
            _db.Employees.Remove(employee);

            // Delete the AppUser (Identity user)
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "User deletion failed", errors = result.Errors });
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new { message = "Employee deleted successfully." });
        }
    }
}

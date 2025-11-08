using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoServiceBackend.Data;
using backend_EAD.DTOs;
using backend_EAD.Models;
using Microsoft.AspNetCore.Identity;
using backend_EAD.Services;

namespace backend_EAD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly NotificationHelper _notificationHelper;

        public EmployeesController(AppDbContext db, UserManager<AppUser> userManager, NotificationHelper notificationHelper)
        {
            _db = db;
            _userManager = userManager;
            _notificationHelper = notificationHelper;
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
        // Update employee - Only allows UserName and PhoneNumber
        // ======================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(string id, [FromBody] UpdateEmployeeDTO dto)
        {
            var employee = await _db.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserID == id);

            if (employee == null)
                return NotFound(new { message = $"Employee with ID '{id}' not found." });

            var updatedFields = new List<string>();

            // Update only allowed fields: UserName and PhoneNumber
            if (!string.IsNullOrWhiteSpace(dto.UserName) && dto.UserName.Trim() != employee.User.UserName)
            {
                employee.User.UserName = dto.UserName.Trim();
                updatedFields.Add("UserName");
            }

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && dto.PhoneNumber.Trim() != employee.User.PhoneNumber)
            {
                employee.User.PhoneNumber = dto.PhoneNumber.Trim();
                updatedFields.Add("PhoneNumber");
            }

            await _db.SaveChangesAsync();

            // Send notification if any fields were updated
            if (updatedFields.Any())
            {
                await _notificationHelper.SendProfileUpdateNotificationAsync(id, "Employee", updatedFields);
            }

            return Ok(new { message = "Employee profile updated successfully.", updatedFields });
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

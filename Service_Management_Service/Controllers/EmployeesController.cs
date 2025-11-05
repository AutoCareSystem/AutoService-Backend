// Controllers/EmployeesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Management_Service.Data;
using Service_Management_Service.Models;
using Service_Management_Service.DTOs;

namespace Service_Management_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeesController(AppDbContext context)
    {
        _context = context;
    }

    // POST: api/employees
    [HttpPost]
    public async Task<ActionResult<Employee>> CreateEmployee([FromBody] CreateEmployeeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // === 1. Validate EmpNo uniqueness ===
        if (await _context.Employees.AnyAsync(e => e.EmpNo == dto.EmpNo))
            return Conflict($"Employee number '{dto.EmpNo}' is already in use.");

        // === 2. Validate Email uniqueness (across all users) ===
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict("A user with this email already exists.");

        // === 3. Create User (shared data) ===
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone
        };

        // === 4. Create Employee ===
        var employee = new Employee
        {
            User = user,
            EmpNo = dto.EmpNo,
            Position = dto.Position,
            IsActive = true
        };

        try
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // === 5. Load User for response ===
            await _context.Entry(employee)
                .Reference(e => e.User)
                .LoadAsync();

            return CreatedAtAction(
                nameof(GetEmployee),
                new { id = employee.UserID },
                employee);
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // GET: api/employees/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetEmployee(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.User)
            .Include(e => e.AssignedAppointments)
            .FirstOrDefaultAsync(e => e.UserID == id);

        if (employee == null)
            return NotFound($"Employee with ID {id} not found.");

        return Ok(employee);
    }

    // GET: api/employees
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetAllEmployees()
    {
        var employees = await _context.Employees
            .Include(e => e.User)
            .Where(e => e.IsActive)
            .OrderBy(e => e.User.Name)
            .ToListAsync();

        return Ok(employees);
    }

    // PATCH: api/employees/{id}/deactivate
    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> DeactivateEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound();

        if (!employee.IsActive)
            return BadRequest("Employee is already inactive.");

        employee.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
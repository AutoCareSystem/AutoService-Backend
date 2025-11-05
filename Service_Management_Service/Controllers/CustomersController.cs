// Controllers/CustomersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Management_Service.Data;
using Service_Management_Service.Models;
using Service_Management_Service.DTOs;


namespace Service_Management_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;

    public CustomersController(AppDbContext db)
    {
        _db = db;
    }

    // POST: api/Customers
    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // === 1. Create User (shared data) ===
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone
        };

        // Validate email uniqueness
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict("A user with this email already exists.");

        // === 2. Create Customer (specific data) ===
        var customer = new Customer
        {
            User = user,
            LoyaltyPoints = 0,
            Address = dto.Address
        };

        try
        {
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            // Load User for response
            await _db.Entry(customer)
                .Reference(c => c.User)
                .LoadAsync();

            return CreatedAtAction(
                nameof(GetCustomer),
                new { id = customer.UserID },
                customer);
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

    // GET: api/Customers/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetCustomer(int id)
    {
        var customer = await _db.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .Include(c => c.Appointments)
            .FirstOrDefaultAsync(c => c.UserID == id);

        if (customer == null)
            return NotFound($"Customer with ID {id} not found.");

        return Ok(customer);
    }

    // GET: api/Customers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAllCustomers()
    {
        var customers = await _db.Customers
            .Include(c => c.User)
            .OrderBy(c => c.User.Name)
            .ToListAsync();

        return Ok(customers);
    }

    // Optional: PATCH to update loyalty points or address
    [HttpPatch("{id}/loyalty")]
    public async Task<IActionResult> AddLoyaltyPoints(int id, [FromBody] AddLoyaltyDto dto)
    {
        if (dto.Points <= 0)
            return BadRequest("Points must be positive.");

        var customer = await _db.Customers.FindAsync(id);
        if (customer == null)
            return NotFound();

        customer.LoyaltyPoints += dto.Points;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
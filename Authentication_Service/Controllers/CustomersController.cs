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
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public CustomersController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // =====================================================
        // GET: api/Customers
        // =====================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetAllCustomers()
        {
            var customers = await _db.Customers
                .Include(c => c.User)
                .Select(c => new CustomerDTO
                {
                    UserID = c.UserID,
                    UserName = c.User.UserName ?? string.Empty,
                    Email = c.User.Email ?? string.Empty,
                    PhoneNumber = c.User.PhoneNumber ?? string.Empty,
                    Role = "Customer", // Always return Customer
                    CreatedAt = c.User.CreatedAt,
                    LoyaltyPoints = c.LoyaltyPoints,
                    Address = c.Address
                })
                .ToListAsync();

            return Ok(customers);
        }

        // =====================================================
        // GET: api/Customers/{id}
        // =====================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDTO>> GetCustomerById(string id)
        {
            var customer = await _db.Customers
                .Include(c => c.User)
                .Where(c => c.UserID == id)
                .Select(c => new CustomerDTO
                {
                    UserID = c.UserID,
                    UserName = c.User.UserName ?? string.Empty,
                    Email = c.User.Email ?? string.Empty,
                    PhoneNumber = c.User.PhoneNumber ?? string.Empty,
                    Role = "Customer",
                    CreatedAt = c.User.CreatedAt,
                    LoyaltyPoints = c.LoyaltyPoints,
                    Address = c.Address
                })
                .FirstOrDefaultAsync();

            if (customer == null)
                return NotFound(new { message = $"Customer with ID '{id}' not found." });

            return Ok(customer);
        }

        // =====================================================
        // PUT: api/Customers/{id}
        // =====================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(string id, [FromBody] UpdateCustomerDTO dto)
        {
            var customer = await _db.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserID == id);

            if (customer == null)
                return NotFound(new { message = $"Customer with ID '{id}' not found." });

            // Update Identity user details
            if (!string.IsNullOrWhiteSpace(dto.UserName))
                customer.User.UserName = dto.UserName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                customer.User.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                customer.User.PhoneNumber = dto.PhoneNumber;

            // Update Customer-specific fields
            customer.LoyaltyPoints = dto.LoyaltyPoints;
            customer.Address = dto.Address;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Customer updated successfully." });
        }

        // =====================================================
        // DELETE: api/Customers/{id}
        // =====================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            var customer = await _db.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserID == id);

            if (customer == null)
                return NotFound(new { message = $"Customer with ID '{id}' not found." });

            // Delete the Customer record
            _db.Customers.Remove(customer);

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

            return Ok(new { message = "Customer deleted successfully." });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoServiceBackend.Data;
using backend_EAD.DTOs;
using backend_EAD.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace backend_EAD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints require authentication
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(AppDbContext db, ILogger<ProfileController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // =====================================================
        // GET: api/Profile
        // Get the authenticated customer's complete profile
        // =====================================================
        [HttpGet]
        public async Task<ActionResult<CustomerProfileResponseDto>> GetMyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token." });

            return await GetCustomerProfile(userId);
        }

        // =====================================================
        // GET: api/Profile/{userId}
        // Get a specific customer's profile (for admin/employee)
        // =====================================================
        [HttpGet("{userId}")]
        public async Task<ActionResult<CustomerProfileResponseDto>> GetCustomerProfileById(string userId)
        {
            // Check if requesting own profile or has admin/employee role
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserId != userId && userRole != "Admin" && userRole != "Employee")
            {
                return Forbid(); // 403 Forbidden
            }

            return await GetCustomerProfile(userId);
        }

        // =====================================================
        // PUT: api/Profile
        // Update authenticated customer's profile
        // =====================================================
        [HttpPut]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateCustomerProfileDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token." });

            return await UpdateCustomerProfile(userId, dto);
        }

        // =====================================================
        // PUT: api/Profile/{userId}
        // Update a specific customer's profile (for admin/employee)
        // =====================================================
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateCustomerProfileById(string userId, [FromBody] UpdateCustomerProfileDto dto)
        {
            // Check if updating own profile or has admin/employee role
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserId != userId && userRole != "Admin" && userRole != "Employee")
            {
                return Forbid(); // 403 Forbidden
            }

            return await UpdateCustomerProfile(userId, dto);
        }

        // =====================================================
        // POST: api/Profile/vehicles
        // Add a new vehicle to authenticated customer's profile
        // =====================================================
        [HttpPost("vehicles")]
        public async Task<ActionResult<VehicleDto>> AddVehicle([FromBody] AddVehicleDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token." });

            // Verify customer exists
            var customer = await _db.Customers.FindAsync(userId);
            if (customer == null)
                return NotFound(new { message = "Customer not found." });

            // Check if VIN already exists
            var existingVehicle = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.Vin == dto.Vin);

            if (existingVehicle != null)
                return BadRequest(new { message = "A vehicle with this VIN already exists." });

            // Create new vehicle
            var vehicle = new Vehicle
            {
                CustomerID = userId,
                Model = dto.Model,
                Year = dto.Year,
                Vin = dto.Vin,
                PlateNumber = dto.PlateNumber,
                Company = dto.Company
            };

            _db.Vehicles.Add(vehicle);
            await _db.SaveChangesAsync();

            var vehicleDto = new VehicleDto
            {
                VehicleID = vehicle.VehicleID,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Vin = vehicle.Vin,
                PlateNumber = vehicle.PlateNumber,
                Company = vehicle.Company
            };

            return CreatedAtAction(nameof(GetMyProfile), new { }, vehicleDto);
        }

        // =====================================================
        // POST: api/Profile/{userId}/vehicles
        // Add a vehicle to a specific customer (for admin/employee)
        // =====================================================
        [HttpPost("{userId}/vehicles")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<VehicleDto>> AddVehicleToCustomer(string userId, [FromBody] AddVehicleDto dto)
        {
            // Verify customer exists
            var customer = await _db.Customers.FindAsync(userId);
            if (customer == null)
                return NotFound(new { message = "Customer not found." });

            // Check if VIN already exists
            var existingVehicle = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.Vin == dto.Vin);

            if (existingVehicle != null)
                return BadRequest(new { message = "A vehicle with this VIN already exists." });

            // Create new vehicle
            var vehicle = new Vehicle
            {
                CustomerID = userId,
                Model = dto.Model,
                Year = dto.Year,
                Vin = dto.Vin,
                PlateNumber = dto.PlateNumber,
                Company = dto.Company
            };

            _db.Vehicles.Add(vehicle);
            await _db.SaveChangesAsync();

            var vehicleDto = new VehicleDto
            {
                VehicleID = vehicle.VehicleID,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Vin = vehicle.Vin,
                PlateNumber = vehicle.PlateNumber,
                Company = vehicle.Company
            };

            return CreatedAtAction(nameof(GetCustomerProfileById), new { userId }, vehicleDto);
        }

        // =====================================================
        // DELETE: api/Profile/vehicles/{vehicleId}
        // Delete a vehicle from authenticated customer's profile
        // =====================================================
        [HttpDelete("vehicles/{vehicleId}")]
        public async Task<IActionResult> DeleteVehicle(int vehicleId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token." });

            var vehicle = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleID == vehicleId && v.CustomerID == userId);

            if (vehicle == null)
                return NotFound(new { message = "Vehicle not found or does not belong to you." });

            // Check if vehicle has appointments
            var hasAppointments = await _db.Set<Appointment>()
                .AnyAsync(a => a.VehicleID == vehicleId);

            if (hasAppointments)
            {
                return BadRequest(new { message = "Cannot delete vehicle with existing appointments." });
            }

            _db.Vehicles.Remove(vehicle);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Vehicle deleted successfully." });
        }

        // =====================================================
        // DELETE: api/Profile/{userId}/vehicles/{vehicleId}
        // Delete a vehicle from a specific customer (for admin/employee)
        // =====================================================
        [HttpDelete("{userId}/vehicles/{vehicleId}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> DeleteVehicleFromCustomer(string userId, int vehicleId)
        {
            var vehicle = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleID == vehicleId && v.CustomerID == userId);

            if (vehicle == null)
                return NotFound(new { message = "Vehicle not found or does not belong to the specified customer." });

            // Check if vehicle has appointments
            var hasAppointments = await _db.Set<Appointment>()
                .AnyAsync(a => a.VehicleID == vehicleId);

            if (hasAppointments)
            {
                return BadRequest(new { message = "Cannot delete vehicle with existing appointments." });
            }

            _db.Vehicles.Remove(vehicle);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Vehicle deleted successfully." });
        }

        // =====================================================
        // HELPER METHODS
        // =====================================================

        private async Task<ActionResult<CustomerProfileResponseDto>> GetCustomerProfile(string userId)
        {
            var customer = await _db.Customers
                .Include(c => c.User)
                .Include(c => c.Vehicles)
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (customer == null)
                return NotFound(new { message = "Customer not found." });

            var profile = new CustomerProfileResponseDto
            {
                UserID = customer.UserID,
                UserName = customer.User.UserName ?? string.Empty,
                Email = customer.User.Email ?? string.Empty,
                PhoneNumber = customer.User.PhoneNumber,
                Address = customer.Address,
                LoyaltyPoints = customer.LoyaltyPoints,
                CreatedAt = customer.User.CreatedAt,
                Vehicles = customer.Vehicles.Select(v => new VehicleDto
                {
                    VehicleID = v.VehicleID,
                    Model = v.Model,
                    Year = v.Year,
                    Vin = v.Vin,
                    PlateNumber = v.PlateNumber,
                    Company = v.Company
                }).ToList()
            };

            return Ok(profile);
        }

        private async Task<IActionResult> UpdateCustomerProfile(string userId, UpdateCustomerProfileDto dto)
        {
            var customer = await _db.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (customer == null)
                return NotFound(new { message = "Customer not found." });

            // Update only allowed fields
            if (!string.IsNullOrWhiteSpace(dto.UserName))
            {
                customer.User.UserName = dto.UserName;
            }

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                customer.User.PhoneNumber = dto.PhoneNumber;
            }

            // Address can be set, updated, or cleared (null)
            if (dto.Address != null)
            {
                customer.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address;
            }

            await _db.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully." });
        }
    }
}

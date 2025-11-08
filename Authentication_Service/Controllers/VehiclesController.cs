using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoServiceBackend.Data;
using backend_EAD.DTOs;
using backend_EAD.Models;

namespace backend_EAD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(AppDbContext db, ILogger<VehiclesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // =====================================================
        // GET: api/Vehicles
        // Get all vehicles in the system (No authentication required)
        // =====================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAllVehicles()
        {
            var vehicles = await _db.Vehicles
                .Select(v => new VehicleDto
                {
                    VehicleID = v.VehicleID,
                    Model = v.Model,
                    Year = v.Year,
                    Vin = v.Vin,
                    PlateNumber = v.PlateNumber,
                    Company = v.Company
                })
                .ToListAsync();

            return Ok(vehicles);
        }

        // =====================================================
        // GET: api/Vehicles/{id}
        // Get a specific vehicle by ID (No authentication required)
        // =====================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleDto>> GetVehicleById(int id)
        {
            var vehicle = await _db.Vehicles
                .Where(v => v.VehicleID == id)
                .Select(v => new VehicleDto
                {
                    VehicleID = v.VehicleID,
                    Model = v.Model,
                    Year = v.Year,
                    Vin = v.Vin,
                    PlateNumber = v.PlateNumber,
                    Company = v.Company
                })
                .FirstOrDefaultAsync();

            if (vehicle == null)
                return NotFound(new { message = "Vehicle not found." });

            return Ok(vehicle);
        }

        // =====================================================
        // GET: api/Vehicles/customer/{customerId}
        // Get all vehicles for a specific customer (No authentication required)
        // =====================================================
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetVehiclesByCustomerId(string customerId)
        {
            var vehicles = await _db.Vehicles
                .Where(v => v.CustomerID == customerId)
                .Select(v => new VehicleDto
                {
                    VehicleID = v.VehicleID,
                    Model = v.Model,
                    Year = v.Year,
                    Vin = v.Vin,
                    PlateNumber = v.PlateNumber,
                    Company = v.Company
                })
                .ToListAsync();

            return Ok(vehicles);
        }

        // =====================================================
        // POST: api/Vehicles
        // Add a new vehicle (No authentication required)
        // Requires CustomerID in the request body
        // =====================================================
        [HttpPost]
        public async Task<ActionResult<VehicleDto>> AddVehicle([FromBody] AddVehicleWithCustomerDto dto)
        {
            // Verify customer exists
            var customer = await _db.Customers.FindAsync(dto.CustomerID);
            if (customer == null)
                return NotFound(new { message = "Customer not found." });

            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.Model))
                return BadRequest(new { message = "Model is required." });

            if (string.IsNullOrWhiteSpace(dto.Year))
                return BadRequest(new { message = "Year is required." });

            if (string.IsNullOrWhiteSpace(dto.Vin))
                return BadRequest(new { message = "VIN is required." });

            if (string.IsNullOrWhiteSpace(dto.PlateNumber))
                return BadRequest(new { message = "Plate number is required." });

            // Check if VIN already exists in the system
            var existingVehicle = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.Vin == dto.Vin.Trim().ToUpper());

            if (existingVehicle != null)
                return BadRequest(new { message = "A vehicle with this VIN already exists in the system." });

            // Check if plate number already exists for this customer
            var existingPlate = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.PlateNumber == dto.PlateNumber.Trim().ToUpper() && v.CustomerID == dto.CustomerID);

            if (existingPlate != null)
                return BadRequest(new { message = "This customer already has a vehicle with this plate number." });

            // Create new vehicle
            var vehicle = new Vehicle
            {
                CustomerID = dto.CustomerID,
                Model = dto.Model.Trim(),
                Year = dto.Year.Trim(),
                Vin = dto.Vin.Trim().ToUpper(),
                PlateNumber = dto.PlateNumber.Trim().ToUpper(),
                Company = string.IsNullOrWhiteSpace(dto.Company) ? null : dto.Company.Trim()
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

            _logger.LogInformation("Added vehicle {VehicleId} for customer {CustomerId} with VIN {Vin}",
                vehicle.VehicleID, dto.CustomerID, vehicle.Vin);

            return CreatedAtAction(nameof(GetVehicleById), new { id = vehicle.VehicleID }, vehicleDto);
        }

        // =====================================================
        // PUT: api/Vehicles/{id}
        // Update a vehicle (No authentication required)
        // =====================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] UpdateVehicleDto dto)
        {
            var vehicle = await _db.Vehicles.FindAsync(id);
            if (vehicle == null)
                return NotFound(new { message = "Vehicle not found." });

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(dto.Model))
                vehicle.Model = dto.Model.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Year))
                vehicle.Year = dto.Year.Trim();

            if (!string.IsNullOrWhiteSpace(dto.PlateNumber))
                vehicle.PlateNumber = dto.PlateNumber.Trim().ToUpper();

            if (dto.Company != null)
                vehicle.Company = string.IsNullOrWhiteSpace(dto.Company) ? null : dto.Company.Trim();

            await _db.SaveChangesAsync();

            _logger.LogInformation("Updated vehicle {VehicleId}", id);

            return Ok(new { message = "Vehicle updated successfully." });
        }

        // =====================================================
        // DELETE: api/Vehicles/{id}
        // Delete a vehicle (No authentication required)
        // =====================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var vehicle = await _db.Vehicles.FindAsync(id);
            if (vehicle == null)
                return NotFound(new { message = "Vehicle not found." });

            // Check if vehicle has any appointments
            var hasAppointments = await _db.Set<Appointment>()
                .AnyAsync(a => a.VehicleID == id);

            if (hasAppointments)
            {
                return BadRequest(new
                {
                    message = "Cannot delete vehicle with existing appointments. Please cancel or complete all appointments first."
                });
            }

            _db.Vehicles.Remove(vehicle);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Deleted vehicle {VehicleId} with VIN {Vin}",
                vehicle.VehicleID, vehicle.Vin);

            return Ok(new
            {
                message = "Vehicle deleted successfully.",
                vehicleId = id,
                vin = vehicle.Vin
            });
        }
    }
}

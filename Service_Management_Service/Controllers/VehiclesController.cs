using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Management_Service.Data;
using Service_Management_Service.Models;

namespace Service_Management_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(AppDbContext context, ILogger<VehiclesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all vehicles for a specific customer
        /// </summary>
        /// <param name="customerId">The customer's unique identifier</param>
        /// <returns>List of vehicles owned by the customer</returns>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetCustomerVehicles(string customerId)
        {
            try
            {
                _logger.LogInformation("🚗 Fetching vehicles for customer: {CustomerId}", customerId);

                // Parse customerId to Guid
                _logger.LogInformation("🚗 Using customer ID as string: {CustomerId}", customerId);

                // Fetch vehicles for the customer
                var vehicles = await _context.Vehicles
                    .Where(v => v.CustomerID == customerId)
                    .OrderBy(v => v.Year)
                    .ThenBy(v => v.Company)
                    .ThenBy(v => v.Model)
                    .ToListAsync();

                _logger.LogInformation("✅ Found {Count} vehicles for customer {CustomerId}", vehicles.Count, customerId);

                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching vehicles for customer {CustomerId}", customerId);
                return StatusCode(500, "Internal server error while fetching vehicles");
            }
        }

        /// <summary>
        /// Get a specific vehicle by ID
        /// </summary>
        /// <param name="vehicleId">The vehicle's unique identifier</param>
        /// <returns>Vehicle details</returns>
        [HttpGet("{vehicleId}")]
        public async Task<ActionResult<Vehicle>> GetVehicle(int vehicleId)
        {
            try
            {
                _logger.LogInformation("🚗 Fetching vehicle details for ID: {VehicleId}", vehicleId);

                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.VehicleID == vehicleId);

                if (vehicle == null)
                {
                    _logger.LogWarning("❌ Vehicle not found: {VehicleId}", vehicleId);
                    return NotFound($"Vehicle with ID {vehicleId} not found");
                }

                _logger.LogInformation("✅ Vehicle found: {Company} {Model} {Year}", vehicle.Company, vehicle.Model, vehicle.Year);

                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching vehicle {VehicleId}", vehicleId);
                return StatusCode(500, "Internal server error while fetching vehicle");
            }
        }

        /// <summary>
        /// Get all vehicles (admin use)
        /// </summary>
        /// <returns>List of all vehicles</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetAllVehicles()
        {
            try
            {
                _logger.LogInformation("🚗 Fetching all vehicles");

                var vehicles = await _context.Vehicles
                    .OrderBy(v => v.Year)
                    .ThenBy(v => v.Company)
                    .ToListAsync();

                _logger.LogInformation("✅ Found {Count} total vehicles", vehicles.Count);

                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error fetching all vehicles");
                return StatusCode(500, "Internal server error while fetching vehicles");
            }
        }

        /// <summary>
        /// Add a new vehicle for a customer
        /// </summary>
        /// <param name="vehicle">Vehicle data</param>
        /// <returns>Created vehicle</returns>
        [HttpPost]
        public async Task<ActionResult<Vehicle>> CreateVehicle([FromBody] Vehicle vehicle)
        {
            try
            {
                _logger.LogInformation("🚗 Creating new vehicle: {Company} {Model} {Year}", vehicle.Company, vehicle.Model, vehicle.Year);

                // Validate customer exists
                var customerExists = await _context.Customers
                    .AnyAsync(c => c.UserID == vehicle.CustomerID);

                if (!customerExists)
                {
                    _logger.LogWarning("❌ Customer not found: {CustomerId}", vehicle.CustomerID);
                    return BadRequest("Customer not found");
                }

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Vehicle created successfully with ID: {VehicleId}", vehicle.VehicleID);

                return CreatedAtAction(nameof(GetVehicle), new { vehicleId = vehicle.VehicleID }, vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating vehicle");
                return StatusCode(500, "Internal server error while creating vehicle");
            }
        }

        /// <summary>
        /// Update an existing vehicle
        /// </summary>
        /// <param name="vehicleId">Vehicle ID</param>
        /// <param name="vehicle">Updated vehicle data</param>
        /// <returns>Updated vehicle</returns>
        [HttpPut("{vehicleId}")]
        public async Task<ActionResult<Vehicle>> UpdateVehicle(int vehicleId, [FromBody] Vehicle vehicle)
        {
            try
            {
                _logger.LogInformation("🚗 Updating vehicle: {VehicleId}", vehicleId);

                if (vehicleId != vehicle.VehicleID)
                {
                    return BadRequest("Vehicle ID mismatch");
                }

                var existingVehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.VehicleID == vehicleId);

                if (existingVehicle == null)
                {
                    _logger.LogWarning("❌ Vehicle not found: {VehicleId}", vehicleId);
                    return NotFound($"Vehicle with ID {vehicleId} not found");
                }

                // Update properties
                existingVehicle.Company = vehicle.Company;
                existingVehicle.Model = vehicle.Model;
                existingVehicle.Year = vehicle.Year;
                existingVehicle.PlateNumber = vehicle.PlateNumber;
                existingVehicle.Vin = vehicle.Vin;

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Vehicle updated successfully: {VehicleId}", vehicleId);

                return Ok(existingVehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating vehicle {VehicleId}", vehicleId);
                return StatusCode(500, "Internal server error while updating vehicle");
            }
        }

        /// <summary>
        /// Delete a vehicle
        /// </summary>
        /// <param name="vehicleId">Vehicle ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{vehicleId}")]
        public async Task<ActionResult> DeleteVehicle(int vehicleId)
        {
            try
            {
                _logger.LogInformation("🚗 Deleting vehicle: {VehicleId}", vehicleId);

                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.VehicleID == vehicleId);

                if (vehicle == null)
                {
                    _logger.LogWarning("❌ Vehicle not found: {VehicleId}", vehicleId);
                    return NotFound($"Vehicle with ID {vehicleId} not found");
                }

                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Vehicle deleted successfully: {VehicleId}", vehicleId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting vehicle {VehicleId}", vehicleId);
                return StatusCode(500, "Internal server error while deleting vehicle");
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AutoServiceBackend.Data;
using backend_EAD.DTOs;
using backend_EAD.Models;

namespace backend_EAD.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProfileController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/Profile/customer/{userId}
    [HttpGet("customer/{userId}")]
    public async Task<ActionResult<CustomerProfileDto>> GetCustomerProfile(int userId)
    {
        // Get customer with user info
        var customer = await _db.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .Include(c => c.Appointments)
                .ThenInclude(a => a.Employee)
                    .ThenInclude(e => e!.User)
            .Include(c => c.Appointments)
                .ThenInclude(a => a.ServiceDetails)
            .Include(c => c.Appointments)
                .ThenInclude(a => a.ProjectDetails)
            .Include(c => c.Appointments)
                .ThenInclude(a => a.AppointmentServices)
                    .ThenInclude(aps => aps.Service)
            .FirstOrDefaultAsync(c => c.UserID == userId);

        if (customer == null)
            return NotFound($"Customer with UserID {userId} not found");

        // Build User Info
        var userInfo = new UserInfoDto
        {
            UserID = customer.User.UserID,
            Name = customer.User.Name,
            Email = customer.User.Email,
            Phone = customer.User.Phone,
            Address = customer.Address,
            LoyaltyPoints = customer.LoyaltyPoints
        };

        // Build Vehicle Info (assuming one vehicle per customer)
        VehicleInfoDto? vehicleInfo = null;
        var vehicle = customer.Vehicles.FirstOrDefault();
        if (vehicle != null)
        {
            vehicleInfo = new VehicleInfoDto
            {
                VehicleID = vehicle.VehicleID,
                PlateNumber = vehicle.PlateNumber,
                Model = vehicle.Model,
                Company = vehicle.Company,
                Year = vehicle.Year,
                Vin = vehicle.Vin
            };
        }

        // Build Upcoming Appointment (most recent active one)
        UpcomingAppointmentDto? upcomingAppointment = null;
        var activeAppointment = customer.Appointments
            .Where(a => a.Status == "Confirmed" || a.Status == "In Progress")
            .OrderBy(a => a.StartDate)
            .FirstOrDefault();

        if (activeAppointment != null)
        {
            string serviceTitle = activeAppointment.AppointmentType == "Service"
                ? (activeAppointment.ServiceDetails?.ServiceOption ?? "Service Appointment")
                : (activeAppointment.ProjectDetails?.ProjectTitle ?? "Project Appointment");

            // Calculate progress percentage (simplified - can be enhanced with time logs)
            int? progressPercentage = null;
            if (activeAppointment.Status == "In Progress")
            {
                // Simple calculation: if in progress, assume 50%
                // You can enhance this with actual time log data
                progressPercentage = 50;
            }
            else if (activeAppointment.Status == "Completed")
            {
                progressPercentage = 100;
            }

            upcomingAppointment = new UpcomingAppointmentDto
            {
                AppointmentID = activeAppointment.AppointmentID,
                ServiceTitle = serviceTitle,
                Date = activeAppointment.StartDate,
                Time = activeAppointment.Time.ToString(@"hh\:mm"),
                Status = activeAppointment.Status,
                ProgressPercentage = progressPercentage,
                AppointmentType = activeAppointment.AppointmentType,
                EmployeeName = activeAppointment.Employee?.User.Name
            };
        }

        // Build Service History (completed appointments)
        var serviceHistory = customer.Appointments
            .Where(a => a.Status == "Completed")
            .OrderByDescending(a => a.StartDate)
            .Select(a => new ServiceHistoryDto
            {
                AppointmentID = a.AppointmentID,
                ServiceTitle = a.AppointmentType == "Service"
                    ? (a.ServiceDetails != null ? a.ServiceDetails.ServiceOption : "Service")
                    : (a.ProjectDetails != null ? a.ProjectDetails.ProjectTitle : "Project"),
                Date = a.StartDate,
                Status = a.Status,
                TotalPrice = a.TotalPrice,
                AppointmentType = a.AppointmentType
            })
            .Take(10)  // Last 10 services
            .ToList();

        // Build Project/Modification Requests
        var projects = customer.Appointments
            .Where(a => a.AppointmentType == "Project")
            .OrderByDescending(a => a.StartDate)
            .Select(a => new ProjectInfoDto
            {
                ProjectID = a.AppointmentID,
                ProjectTitle = a.ProjectDetails != null ? a.ProjectDetails.ProjectTitle : "Untitled Project",
                ProjectDescription = a.ProjectDetails?.ProjectDescription,
                Status = a.Status,
                RequestedAt = a.StartDate,
                AssignedEmployee = a.Employee != null ? a.Employee.User.Name : null,
                TotalPrice = a.TotalPrice
            })
            .ToList();

        // Build complete profile
        var profile = new CustomerProfileDto
        {
            User = userInfo,
            Vehicle = vehicleInfo,
            UpcomingAppointment = upcomingAppointment,
            ServiceHistory = serviceHistory,
            Projects = projects
        };

        return Ok(profile);
    }

    // PUT: api/Profile/customer/{userId}
    [HttpPut("customer/{userId}")]
    public async Task<IActionResult> UpdateCustomerProfile(int userId, [FromBody] UpdateProfileDto dto)
    {
        var customer = await _db.Customers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserID == userId);

        if (customer == null)
            return NotFound($"Customer with UserID {userId} not found");

        // Update User fields
        if (!string.IsNullOrWhiteSpace(dto.Name))
            customer.User.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.Phone))
            customer.User.Phone = dto.Phone;

        // Update Customer fields
        if (!string.IsNullOrWhiteSpace(dto.Address))
            customer.Address = dto.Address;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Profile updated successfully" });
    }

    // PUT: api/Profile/customer/{userId}/vehicle
    [HttpPut("customer/{userId}/vehicle")]
    public async Task<IActionResult> UpdateOrAddVehicle(int userId, [FromBody] UpdateVehicleDto dto)
    {
        var customer = await _db.Customers
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.UserID == userId);

        if (customer == null)
            return NotFound($"Customer with UserID {userId} not found");

        // Check if customer already has a vehicle
        var existingVehicle = customer.Vehicles.FirstOrDefault();

        if (existingVehicle != null)
        {
            // Update existing vehicle
            existingVehicle.Model = dto.Model;
            existingVehicle.Year = dto.Year;
            existingVehicle.Vin = dto.Vin;
            existingVehicle.PlateNumber = dto.PlateNumber;
            existingVehicle.Company = dto.Company;
        }
        else
        {
            // Add new vehicle
            var newVehicle = new Vehicle
            {
                CustomerID = customer.UserID,
                Model = dto.Model,
                Year = dto.Year,
                Vin = dto.Vin,
                PlateNumber = dto.PlateNumber,
                Company = dto.Company
            };
            _db.Vehicles.Add(newVehicle);
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "Vehicle information updated successfully" });
    }

    // GET: api/Profile/customer/{userId}/loyalty
    [HttpGet("customer/{userId}/loyalty")]
    public async Task<ActionResult<object>> GetLoyaltyPoints(int userId)
    {
        var customer = await _db.Customers.FindAsync(userId);

        if (customer == null)
            return NotFound($"Customer with UserID {userId} not found");

        return Ok(new { loyaltyPoints = customer.LoyaltyPoints });
    }

    // POST: api/Profile/customer/{userId}/loyalty/add
    [HttpPost("customer/{userId}/loyalty/add")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> AddLoyaltyPoints(int userId, [FromBody] AddLoyaltyPointsDto dto)
    {
        if (dto.Points <= 0)
            return BadRequest("Points must be positive");

        var customer = await _db.Customers.FindAsync(userId);

        if (customer == null)
            return NotFound($"Customer with UserID {userId} not found");

        customer.LoyaltyPoints += dto.Points;
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Added {dto.Points} loyalty points", totalPoints = customer.LoyaltyPoints });
    }

    // GET: api/Profile/customers (Admin only - get all customers)
    [HttpGet("customers")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<UserInfoDto>>> GetAllCustomers()
    {
        var customers = await _db.Customers
            .Include(c => c.User)
            .Select(c => new UserInfoDto
            {
                UserID = c.UserID,
                Name = c.User.Name,
                Email = c.User.Email,
                Phone = c.User.Phone,
                Address = c.Address,
                LoyaltyPoints = c.LoyaltyPoints
            })
            .ToListAsync();

        return Ok(customers);
    }
}

public record AddLoyaltyPointsDto(int Points);

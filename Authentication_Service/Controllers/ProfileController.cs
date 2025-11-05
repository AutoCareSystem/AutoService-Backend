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

    // GET: api/Profile/employee/{userId}
    [HttpGet("employee/{userId}")]
    public async Task<ActionResult<EmployeeProfileDto>> GetEmployeeProfile(int userId)
    {
        var employee = await _db.Employees
            .Include(e => e.User)
            .Include(e => e.Appointments)
                .ThenInclude(a => a.Customer)
                    .ThenInclude(c => c.User)
            .Include(e => e.Appointments)
                .ThenInclude(a => a.ServiceDetails)
            .Include(e => e.Appointments)
                .ThenInclude(a => a.ProjectDetails)
            .Include(e => e.Appointments)
                .ThenInclude(a => a.Customer)
                    .ThenInclude(c => c.Vehicles)
            .FirstOrDefaultAsync(e => e.UserID == userId);

        if (employee == null)
            return NotFound($"Employee with UserID {userId} not found");

        // Calculate statistics
        var totalAppointments = employee.Appointments.Count;
        var completedAppointments = employee.Appointments.Count(a => a.Status == "Completed");

        // Get recent appointments (last 10)
        var recentAppointments = employee.Appointments
            .OrderByDescending(a => a.StartDate)
            .Take(10)
            .Select(a => new EmployeeAppointmentDto
            {
                AppointmentID = a.AppointmentID,
                CustomerName = a.Customer.User.Name,
                AppointmentType = a.AppointmentType,
                ServiceTitle = a.AppointmentType == "Service"
                    ? (a.ServiceDetails?.ServiceOption ?? "Service Appointment")
                    : (a.ProjectDetails?.ProjectTitle ?? "Project Appointment"),
                Date = a.StartDate,
                Time = a.Time.ToString(@"hh\:mm"),
                Status = a.Status,
                VehicleInfo = a.Customer.Vehicles.FirstOrDefault() != null
                    ? $"{a.Customer.Vehicles.First().Company} {a.Customer.Vehicles.First().Model} ({a.Customer.Vehicles.First().PlateNumber})"
                    : null
            })
            .ToList();

        var profile = new EmployeeProfileDto
        {
            EmployeeID = employee.UserID,
            UserID = employee.UserID,
            Name = employee.User.Name,
            Email = employee.User.Email,
            Phone = employee.User.Phone,
            Position = employee.Position,
            HourlyRate = employee.HourlyRate,
            TotalAppointments = totalAppointments,
            CompletedAppointments = completedAppointments,
            RecentAppointments = recentAppointments
        };

        return Ok(profile);
    }

    // PUT: api/Profile/employee/{userId}
    [HttpPut("employee/{userId}")]
    public async Task<IActionResult> UpdateEmployeeProfile(int userId, [FromBody] UpdateEmployeeProfileDto dto)
    {
        var employee = await _db.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.UserID == userId);

        if (employee == null)
            return NotFound($"Employee with UserID {userId} not found");

        // Update User fields
        if (!string.IsNullOrWhiteSpace(dto.Name))
            employee.User.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.Phone))
            employee.User.Phone = dto.Phone;

        // Update Employee-specific fields
        if (!string.IsNullOrWhiteSpace(dto.Position))
            employee.Position = dto.Position;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Employee profile updated successfully" });
    }

    // GET: api/Profile/employees (Admin only - get all employees)
    [HttpGet("employees")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<object>>> GetAllEmployees()
    {
        var employees = await _db.Employees
            .Include(e => e.User)
            .Include(e => e.Appointments)
            .Select(e => new
            {
                EmployeeID = e.UserID,
                Name = e.User.Name,
                Email = e.User.Email,
                Phone = e.User.Phone,
                Position = e.Position,
                HourlyRate = e.HourlyRate,
                TotalAppointments = e.Appointments.Count,
                CompletedAppointments = e.Appointments.Count(a => a.Status == "Completed")
            })
            .ToListAsync();

        return Ok(employees);
    }
}

public record AddLoyaltyPointsDto(int Points);

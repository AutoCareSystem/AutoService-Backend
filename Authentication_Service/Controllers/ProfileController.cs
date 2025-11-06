using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<AppUser> _userManager;

    public ProfileController(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET: api/Profile/customer/{userId}
    [HttpGet("customer/{userId}")]
    public async Task<ActionResult<CustomerProfileDto>> GetCustomerProfile(string userId)
    {
        var customer = await _db.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .Include(c => c.Appointments)
                .ThenInclude(a => a.Employee!)
                    .ThenInclude(e => e!.User)
            .Include(c => c.Appointments)
                .ThenInclude(a => a.ServiceDetails)
            .Include(c => c.Appointments)
                .ThenInclude(a => a.ProjectDetails)
            .Include(c => c.Appointments)
                .ThenInclude(a => a.AppointmentServices!)
                    .ThenInclude(aps => aps.Service)
            .FirstOrDefaultAsync(c => c.UserID == userId);

        if (customer == null)
            return NotFound($"Customer with UserID {userId} not found");

        var userInfo = new UserInfoDto
        {
            UserID = int.Parse(customer.User.Id),
            Name = customer.User.UserName ?? "User",
            Email = customer.User.Email ?? "",
            Phone = customer.User.PhoneNumber ?? "",
            Address = customer.Address,
            LoyaltyPoints = customer.LoyaltyPoints
        };

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

            int? progressPercentage = activeAppointment.Status == "In Progress" ? 50 :
                                      activeAppointment.Status == "Completed" ? 100 : null;

            upcomingAppointment = new UpcomingAppointmentDto
            {
                AppointmentID = activeAppointment.AppointmentID,
                ServiceTitle = serviceTitle,
                Date = activeAppointment.StartDate,
                Time = activeAppointment.Time.ToString(@"hh\:mm"),
                Status = activeAppointment.Status,
                ProgressPercentage = progressPercentage,
                AppointmentType = activeAppointment.AppointmentType,
                EmployeeName = activeAppointment.Employee?.User.UserName
            };
        }

        var serviceHistory = customer.Appointments
            .Where(a => a.Status == "Completed")
            .OrderByDescending(a => a.StartDate)
            .Select(a => new ServiceHistoryDto
            {
                AppointmentID = a.AppointmentID,
                ServiceTitle = a.AppointmentType == "Service"
                    ? (a.ServiceDetails?.ServiceOption ?? "Service")
                    : (a.ProjectDetails?.ProjectTitle ?? "Project"),
                Date = a.StartDate,
                Status = a.Status,
                TotalPrice = a.TotalPrice,
                AppointmentType = a.AppointmentType
            })
            .Take(10)
            .ToList();

        var projects = customer.Appointments
            .Where(a => a.AppointmentType == "Project")
            .OrderByDescending(a => a.StartDate)
            .Select(a => new ProjectInfoDto
            {
                ProjectID = a.AppointmentID,
                ProjectTitle = a.ProjectDetails?.ProjectTitle ?? "Untitled Project",
                ProjectDescription = a.ProjectDetails?.ProjectDescription,
                Status = a.Status,
                RequestedAt = a.StartDate,
                AssignedEmployee = a.Employee?.User.UserName,
                TotalPrice = a.TotalPrice
            })
            .ToList();

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
    public async Task<IActionResult> UpdateCustomerProfile(string userId, [FromBody] UpdateProfileDto dto)
    {
        var customer = await _db.Customers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserID == userId);

        if (customer == null)
            return NotFound($"Customer with UserID {userId} not found");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            customer.User.UserName = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.Phone))
            customer.User.PhoneNumber = dto.Phone;

        if (!string.IsNullOrWhiteSpace(dto.Address))
            customer.Address = dto.Address;

        await _userManager.UpdateAsync(customer.User);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Profile updated successfully" });
    }

    // PUT: api/Profile/customer/{userId}/vehicle
    [HttpPut("customer/{userId}/vehicle")]
    public async Task<IActionResult> UpdateOrAddVehicle(string userId, [FromBody] UpdateVehicleDto dto)
    {
        var customer = await _db.Customers
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.UserID == userId);

        if (customer == null)
            return NotFound($"Customer with UserID {userId} not found");

        var existingVehicle = customer.Vehicles.FirstOrDefault();
        if (existingVehicle != null)
        {
            existingVehicle.Model = dto.Model;
            existingVehicle.Year = dto.Year;
            existingVehicle.Vin = dto.Vin;
            existingVehicle.PlateNumber = dto.PlateNumber;
            existingVehicle.Company = dto.Company;
        }
        else
        {
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
    public async Task<ActionResult<object>> GetLoyaltyPoints(string userId)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.UserID == userId);
        if (customer == null)
            return NotFound($"Customer with UserID {userId} not found");

        return Ok(new { loyaltyPoints = customer.LoyaltyPoints });
    }

    // POST: api/Profile/customer/{userId}/loyalty/add
    [HttpPost("customer/{userId}/loyalty/add")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> AddLoyaltyPoints(string userId, [FromBody] AddLoyaltyPointsDto dto)
    {
        if (dto.Points <= 0)
            return BadRequest("Points must be positive");

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.UserID == userId);
        if (customer == null)
            return NotFound($"Customer with UserID {userId} not found");

        customer.LoyaltyPoints += dto.Points;
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Added {dto.Points} loyalty points", totalPoints = customer.LoyaltyPoints });
    }

    // GET: api/Profile/customers
    [HttpGet("customers")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<UserInfoDto>>> GetAllCustomers()
    {
        var customers = await _db.Customers
            .Include(c => c.User)
            .Select(c => new UserInfoDto
            {
                UserID = int.Parse(c.User.Id),
                Name = c.User.UserName ?? "",
                Email = c.User.Email ?? "",
                Phone = c.User.PhoneNumber ?? "",
                Address = c.Address,
                LoyaltyPoints = c.LoyaltyPoints
            })
            .ToListAsync();

        return Ok(customers);
    }

    // GET: api/Profile/employee/{userId}
    [HttpGet("employee/{userId}")]
    public async Task<ActionResult<EmployeeProfileDto>> GetEmployeeProfile(string userId)
    {
        var employee = await _db.Employees
            .Include(e => e.User)
            .Include(e => e.AssignedAppointments)
                .ThenInclude(a => a.Customer!)
                    .ThenInclude(c => c!.User)
            .Include(e => e.AssignedAppointments)
                .ThenInclude(a => a.ServiceDetails)
            .Include(e => e.AssignedAppointments)
                .ThenInclude(a => a.ProjectDetails)
            .Include(e => e.AssignedAppointments)
                .ThenInclude(a => a.Customer!)
                    .ThenInclude(c => c!.Vehicles)
            .FirstOrDefaultAsync(e => e.UserID == userId); // Fixed: UserID

        if (employee == null)
            return NotFound($"Employee with UserID {userId} not found");

        var totalAppointments = employee.AssignedAppointments.Count;
        var completedAppointments = employee.AssignedAppointments.Count(a => a.Status == "Completed");

        var recentAppointments = employee.AssignedAppointments
            .OrderByDescending(a => a.StartDate)
            .Take(10)
            .Select(a => new EmployeeAppointmentDto
            {
                AppointmentID = a.AppointmentID,
                CustomerName = a.Customer!.User.UserName ?? "Unknown",
                AppointmentType = a.AppointmentType,
                ServiceTitle = a.AppointmentType == "Service"
                    ? (a.ServiceDetails?.ServiceOption ?? "Service")
                    : (a.ProjectDetails?.ProjectTitle ?? "Project"),
                Date = a.StartDate,
                Time = a.Time.ToString(@"hh\:mm"),
                Status = a.Status,
                VehicleInfo = a.Customer.Vehicles.FirstOrDefault() is { } v
                    ? $"{v.Company} {v.Model} ({v.PlateNumber})"
                    : null
            })
            .ToList();

        var profile = new EmployeeProfileDto
        {
            EmployeeID = int.Parse(employee.User.Id),
            UserID = int.Parse(employee.User.Id),
            Name = employee.User.UserName ?? "",
            Email = employee.User.Email ?? "",
            Phone = employee.User.PhoneNumber ?? "",
            Position = employee.Position,
            HourlyRate = 0m,
            TotalAppointments = totalAppointments,
            CompletedAppointments = completedAppointments,
            RecentAppointments = recentAppointments
        };

        return Ok(profile);
    }

    // PUT: api/Profile/employee/{userId}
    [HttpPut("employee/{userId}")]
    public async Task<IActionResult> UpdateEmployeeProfile(string userId, [FromBody] UpdateEmployeeProfileDto dto)
    {
        var employee = await _db.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.UserID == userId); // Fixed: UserID

        if (employee == null)
            return NotFound($"Employee with UserID {userId} not found");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            employee.User.UserName = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.Phone))
            employee.User.PhoneNumber = dto.Phone;

        if (!string.IsNullOrWhiteSpace(dto.Position))
            employee.Position = dto.Position;

        await _userManager.UpdateAsync(employee.User);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Employee profile updated successfully" });
    }

    // GET: api/Profile/employees
    [HttpGet("employees")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<object>>> GetAllEmployees()
    {
        var employees = await _db.Employees
            .Include(e => e.User)
            .Include(e => e.AssignedAppointments)
            .Select(e => new
            {
                EmployeeID = int.Parse(e.User.Id),
                Name = e.User.UserName,
                Email = e.User.Email,
                Phone = e.User.PhoneNumber,
                Position = e.Position,
                EmpNo = e.EmpNo,
                IsActive = e.IsActive,
                TotalAppointments = e.AssignedAppointments.Count,
                CompletedAppointments = e.AssignedAppointments.Count(a => a.Status == "Completed")
            })
            .ToListAsync();

        return Ok(employees);
    }
}

public record AddLoyaltyPointsDto(int Points);
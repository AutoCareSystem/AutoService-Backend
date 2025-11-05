// Controllers/AppointmentsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Management_Service.Data;
using Service_Management_Service.Models;
using Service_Management_Service.DTOs;

namespace Service_Management_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AppointmentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<Appointment>> CreateAppointment(CreateAppointmentDto dto)
    {
        // === 1. Validate AppointmentType ===
        if (!new[] { "Service", "Project" }.Contains(dto.AppointmentType))
            return BadRequest("AppointmentType must be 'Service' or 'Project'.");

        // === 2. Validate Customer ===
        var customer = await _context.Customers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserID == dto.CustomerID);
        if (customer == null)
            return NotFound($"Customer with ID {dto.CustomerID} not found.");

        // === 3. Validate Vehicle ===
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.VehicleID == dto.VehicleID && v.CustomerID == dto.CustomerID);
        if (vehicle == null)
            return BadRequest("Vehicle not found or does not belong to the customer.");

        // === 5. Validate EndDate ===
        if (dto.EndDate.HasValue && dto.EndDate.Value <= dto.StartDate.Date.Add(dto.Time))
            return BadRequest("EndDate must be after StartDate + Time.");

        // === 6. Create Base Appointment ===
        var appointment = new Appointment
        {
            CustomerID = dto.CustomerID,
            VehicleID = dto.VehicleID,
            EmployeeID = dto.EmployeeID,
            StartDate = dto.StartDate.Date,
            Time = dto.Time,
            EndDate = dto.EndDate,
            Status = "Pending",
            AppointmentType = dto.AppointmentType
        };

        // ==================== SERVICE LOGIC ====================
        if (dto.AppointmentType == "Service")
        {
            if (string.IsNullOrWhiteSpace(dto.ServiceOption))
                return BadRequest("ServiceOption is required for Service appointments.");

            if (!new[] { "Full", "Half", "Custom" }.Contains(dto.ServiceOption))
                return BadRequest("ServiceOption must be 'Full', 'Half', or 'Custom'.");

            // Create ServiceAppointment (always)
            appointment.ServiceDetails = new ServiceAppointment
            {
                ServiceOption = dto.ServiceOption
            };

            // --- Full / Half: Use ServicePackage ---
            if (dto.ServiceOption is "Full" or "Half")
            {
                if (!dto.ServicePackageID.HasValue)
                    return BadRequest($"{dto.ServiceOption} requires ServicePackageID.");

                var package = await _context.ServicePackages
                    .Include(p => p.Items)
                        .ThenInclude(i => i.Service)
                    .FirstOrDefaultAsync(p =>
                        p.ServicePackageID == dto.ServicePackageID.Value &&
                        p.PackageType == dto.ServiceOption);

                if (package == null)
                    return NotFound($"No {dto.ServiceOption} package found with ID {dto.ServicePackageID}.");

                appointment.ServiceDetails.ServicePackageID = package.ServicePackageID;
                appointment.TotalPrice = package.Price;
            }
            // --- Custom: Use AppointmentServices ---
            else if (dto.ServiceOption == "Custom")
            {
                if (dto.CustomServiceIDs == null || !dto.CustomServiceIDs.Any())
                    return BadRequest("CustomServiceIDs are required for Custom appointments.");

                var validServiceIds = await _context.Services
                    .Where(s => dto.CustomServiceIDs.Contains(s.ServiceID) && s.Status == "Active")
                    .Select(s => s.ServiceID)
                    .ToListAsync();

                if (validServiceIds.Count != dto.CustomServiceIDs.Count)
                    return BadRequest("One or more Service IDs are invalid or inactive.");

                foreach (var serviceId in dto.CustomServiceIDs)
                {
                    var service = await _context.Services.FindAsync(serviceId);
                    if (service != null)
                    {
                        appointment.AppointmentServices.Add(new AppointmentService
                        {
                            ServiceID = serviceId,
                            CustomPrice = service.Price
                        });
                    }
                }

                appointment.TotalPrice = appointment.AppointmentServices
                    .Sum(aps => aps.CustomPrice ?? 0m);
            }
        }
        // ==================== PROJECT LOGIC ====================
        else if (dto.AppointmentType == "Project")
        {
            if (string.IsNullOrWhiteSpace(dto.ProjectTitle))
                return BadRequest("ProjectTitle is required for Project appointments.");

            appointment.ProjectDetails = new ProjectAppointment
            {
                ProjectTitle = dto.ProjectTitle,
                ProjectDescription = dto.ProjectDescription
            };
        }

        // ==================== SAVE ====================
        try
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // === Load Navigation Properties ===
            await _context.Entry(appointment)
                .Reference(a => a.Customer)
                .Query()
                .Include(c => c.User)
                .LoadAsync();

            await _context.Entry(appointment)
                .Reference(a => a.Vehicle)
                .LoadAsync();

            if (appointment.EmployeeID.HasValue)
            {
                await _context.Entry(appointment)
                    .Reference(a => a.Employee)
                    .Query()
                    .Include(e => e.User)
                    .LoadAsync();
            }

            if (appointment.ServiceDetails != null)
            {
                await _context.Entry(appointment.ServiceDetails)
                    .Reference(s => s.ServicePackage)
                    .Query()
                    .Include(p => p.Items)
                    .ThenInclude(i => i.Service)
                    .LoadAsync();
            }

            if (appointment.ProjectDetails != null)
            {
                await _context.Entry(appointment)
                    .Reference(a => a.ProjectDetails)
                    .LoadAsync();
            }

            await _context.Entry(appointment)
                .Collection(a => a.AppointmentServices)
                .Query()
                .Include(aps => aps.Service)
                .LoadAsync();

            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.AppointmentID }, appointment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // ==================== GET SINGLE ====================
    [HttpGet("{id}")]
    public async Task<ActionResult<Appointment>> GetAppointment(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Customer)
                .ThenInclude(c => c.User)
            .Include(a => a.Vehicle)
            .Include(a => a.Employee)
                .ThenInclude(e => e.User)
            .Include(a => a.ServiceDetails)
                .ThenInclude(s => s.ServicePackage)
                .ThenInclude(p => p.Items)
                .ThenInclude(i => i.Service)
            .Include(a => a.ProjectDetails)
            .Include(a => a.AppointmentServices)
                .ThenInclude(aps => aps.Service)
            .FirstOrDefaultAsync(a => a.AppointmentID == id);

        if (appointment == null)
            return NotFound();

        return Ok(appointment);
    }

    // GET: api/appointments?status=Pending&type=Service&employeeId=42
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments(
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] int? employeeId)
    {
        var query = _context.Appointments
            .Include(a => a.Customer).ThenInclude(c => c.User)
            .Include(a => a.Vehicle)
            .Include(a => a.Employee).ThenInclude(e => e.User)
            .Include(a => a.ServiceDetails)
                .ThenInclude(s => s.ServicePackage)
                .ThenInclude(p => p.Items)
                .ThenInclude(i => i.Service)
            .Include(a => a.ProjectDetails)
            .Include(a => a.AppointmentServices)
                .ThenInclude(aps => aps.Service)
            .AsQueryable();

        // === FILTER: Status (case-insensitive) ===
        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusNorm = status.Trim().ToLower();
            query = query.Where(a => a.Status != null && a.Status.ToLower() == statusNorm);
        }

        // === FILTER: AppointmentType ===
        if (!string.IsNullOrWhiteSpace(type))
        {
            var typeNorm = type.Trim().ToLower();
            if (!new[] { "Service", "Project" }.Contains(typeNorm, StringComparer.OrdinalIgnoreCase))
                return BadRequest("Invalid type. Must be 'Service' or 'Project'.");

            query = query.Where(a => a.AppointmentType.ToLower() == typeNorm);
        }

        // === FILTER: EmployeeID (only assigned) ===
        if (employeeId.HasValue)
        {
            query = query.Where(a => a.EmployeeID == employeeId.Value);
        }

        // === ORDERING ===
        query = query
            .OrderBy(a => a.StartDate)
            .ThenBy(a => a.Time);

        // === EXECUTE ===
        var appointments = await query.ToListAsync();

        return Ok(appointments);
    }

    // GET: api/appointments/customer/9/vehicle/8/summary
    [HttpGet("customer/{customerId}/vehicle/{vehicleId}/summary")]
    public async Task<ActionResult<CustomerVehicleSummaryDto>> GetCustomerVehicleSummary(
        int customerId,
        int vehicleId)
    {
        // === 1. Validate: Vehicle exists and belongs to Customer ===
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.VehicleID == vehicleId && v.CustomerID == customerId);

        if (vehicle == null)
            return NotFound("Vehicle not found or does not belong to the customer.");

        // === 2. Count total vehicles owned by the customer ===
        var totalVehicles = await _context.Vehicles
            .CountAsync(v => v.CustomerID == customerId);

        // === 3. Fetch all appointments for this vehicle ===
        var appointments = await _context.Appointments
            .Where(a => a.CustomerID == customerId && a.VehicleID == vehicleId)
            .Include(a => a.ServiceDetails)
            .Include(a => a.AppointmentServices)
            .ToListAsync();

        // === 4. Initialize result with TotalVehicles ===
        var result = new CustomerVehicleSummaryDto
        {
            TotalVehicles = totalVehicles  //  Total vehicles owned
        };

        // === 5. Loop through appointments and calculate stats ===
        foreach (var appt in appointments)
        {
            // TOTAL SPENT (only Completed)
            if (appt.Status == "Completed" && appt.TotalPrice.HasValue)
            {
                result.TotalSpent += appt.TotalPrice.Value;
            }

            // COMPLETED COUNT
            if (appt.Status == "Completed")
            {
                result.CompletedCount += CountServicesInAppointment(appt);
            }

            // PENDING COUNT (Pending or Accepted)
            if (appt.Status is "Pending" or "Accepted")
            {
                result.PendingCount += CountServicesInAppointment(appt);
            }
        }

        return Ok(result);
    }


    private int CountServicesInAppointment(Appointment appt)
    {
        // Project = 1
        if (appt.AppointmentType == "Project")
            return 1;

        // Full / Half Package
        if (appt.ServiceDetails?.ServiceOption is "Full" or "Half")
        {
            if (appt.ServiceDetails.ServicePackageID.HasValue)
            {
                // Use sync count if ServicePackageItems already loaded, or async
                return _context.ServicePackageItems
                    .Count(i => i.ServicePackageID == appt.ServiceDetails.ServicePackageID.Value);
            }
        }
        // Custom
        else if (appt.ServiceDetails?.ServiceOption == "Custom")
        {
            return appt.AppointmentServices.Count;
        }

        return 0;
    }

}
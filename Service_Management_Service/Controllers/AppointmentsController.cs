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

        // === 4. Validate Employee (if provided) ===
        if (!string.IsNullOrWhiteSpace(dto.EmployeeID))
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserID == dto.EmployeeID);
            if (employee == null)
                return NotFound($"Employee with ID {dto.EmployeeID} not found.");
        }

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

        // ==================== SAVE & LOAD ====================
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

            if (!string.IsNullOrWhiteSpace(appointment.EmployeeID))
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

            return CreatedAtAction(
                nameof(GetAppointment),
                new { id = appointment.AppointmentID },
                appointment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Appointment>> GetAppointment(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Customer).ThenInclude(c => c.User)
            .Include(a => a.Employee).ThenInclude(e => e.User)
            .Include(a => a.Vehicle)
            .FirstOrDefaultAsync(a => a.AppointmentID == id);

        if (appointment == null) return NotFound();
        return Ok(appointment);
    }

    // GET: api/appointments?status=Pending&type=Service&employeeId=f003b7d9-eefe-4cb6-8f87-06ff62c54d8a
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments(
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] string? employeeId)
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

        // === FILTER: AppointmentType (validated & case-insensitive) ===
        if (!string.IsNullOrWhiteSpace(type))
        {
            var typeNorm = type.Trim().ToLower();
            if (!new[] { "service", "project" }.Contains(typeNorm))
                return BadRequest("Invalid type. Must be 'Service' or 'Project'.");

            query = query.Where(a => a.AppointmentType.ToLower() == typeNorm);
        }

        // === FILTER: EmployeeID (string GUID, only if assigned) ===
        if (!string.IsNullOrWhiteSpace(employeeId))
        {
            query = query.Where(a => a.EmployeeID == employeeId);
        }

        // === ORDERING: StartDate  Time ===
        query = query
            .OrderBy(a => a.StartDate)
            .ThenBy(a => a.Time);

        // === EXECUTE ===
        var appointments = await query.ToListAsync();
        return Ok(appointments);
    }

    // GET: api/appointments/customer/f003b7d9-eefe-4cb6-8f87-06ff62c54d8a/vehicle/8/summary
    [HttpGet("customer/{customerId}/vehicle/{vehicleId}/summary")]
    public async Task<ActionResult<CustomerVehicleSummaryDto>> GetCustomerVehicleSummary(
        string customerId,
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
                .ThenInclude(s => s.ServicePackage)
                .ThenInclude(p => p.Items)
                .ThenInclude(i => i.Service)
            .Include(a => a.ProjectDetails)
            .Include(a => a.AppointmentServices)
                .ThenInclude(aps => aps.Service)
            .Include(a => a.Employee)
                .ThenInclude(e => e.User)
            .ToListAsync();

        // === 4. Initialize result ===
        var result = new CustomerVehicleSummaryDto
        {
            TotalVehicles = totalVehicles
        };

        // === 5. Calculate stats from appointments ===
        foreach (var appt in appointments)
        {
            // TOTAL SPENT (only Completed)
            if (appt.Status == "Completed" && appt.TotalPrice.HasValue)
            {
                result.TotalSpent += appt.TotalPrice.Value;
            }

            // COMPLETED & PENDING SERVICE COUNTS
            int serviceCount = CountServicesInAppointment(appt);

            if (appt.Status == "Completed")
            {
                result.CompletedCount += serviceCount;
            }
            else if (appt.Status is "Pending" or "Accepted")
            {
                result.PendingCount += serviceCount;
            }
        }

        return Ok(result);
    }

    // PUT: api/appointments/123/accept
    [HttpPut("{id}/accept")]
    public async Task<IActionResult> AcceptAppointment(int id, [FromBody] AcceptAppointmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // === 1. Load appointment with Employee ===
        var appointment = await _context.Appointments
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.AppointmentID == id);

        if (appointment == null)
            return NotFound("Appointment not found.");

        // === 2. Check if already assigned ===
        if (!string.IsNullOrWhiteSpace(appointment.EmployeeID))
            return BadRequest("Appointment is already assigned.");

        // === 3. Only allow Pending appointments ===
        if (appointment.Status != "Pending")
            return BadRequest("Only Pending appointments can be accepted.");

        // === 4. Validate Employee (active & exists) ===
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserID == dto.EmployeeID && e.IsActive);

        if (employee == null)
            return BadRequest("Invalid or inactive employee.");

        // === 5. Update Appointment ===
        appointment.EmployeeID = dto.EmployeeID;
        appointment.Status = "Approved";

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Appointment accepted.",
                status = "Approved",
                employeeId = dto.EmployeeID
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    // PUT: api/appointments/{id}/complete
    [HttpPut("{id}/complete")]
    public async Task<IActionResult> CompleteAppointment(int id, [FromBody] CompleteAppointmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // === 1. Load appointment with Employee ===
        var appointment = await _context.Appointments
            .Include(a => a.Employee)
                .ThenInclude(e => e.User)
            .FirstOrDefaultAsync(a => a.AppointmentID == id);

        if (appointment == null)
            return NotFound("Appointment not found.");

        // === 2. Must be assigned ===
        if (string.IsNullOrWhiteSpace(appointment.EmployeeID))
            return BadRequest("Appointment must be assigned to an employee.");

        // === 3. Employee must match ===
        if (appointment.EmployeeID != dto.EmployeeID)
            return Forbid("You can only complete your own appointments.");

        // === 4. Status checks ===
        if (appointment.Status == "Completed")
            return BadRequest("Appointment is already completed.");

        if (appointment.Status != "InProgress")
            return BadRequest("Only InProgress appointments can be completed.");

        // === 5. Mark as Completed + Set EndDate to NOW ===
        appointment.Status = "Completed";
        appointment.EndDate = DateTime.Now;  

        try
        {
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Appointment completed successfully.",
                status = "Completed",
                employeeId = dto.EmployeeID,
                completedAt = appointment.EndDate.Value.ToString("yyyy-MM-dd HH:mm:ss") 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    // PUT: api/appointments/123/start
    [HttpPut("{id}/start")]
    public async Task<IActionResult> StartAppointment(int id, [FromBody] StartAppointmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // === 1. Load appointment with Employee ===
        var appointment = await _context.Appointments
            .Include(a => a.Employee)
                .ThenInclude(e => e.User)
            .FirstOrDefaultAsync(a => a.AppointmentID == id);

        if (appointment == null)
            return NotFound("Appointment not found.");

        // === 2. Must be assigned ===
        if (string.IsNullOrWhiteSpace(appointment.EmployeeID))
            return BadRequest("Appointment must be assigned to an employee.");

        // === 3. Employee must match ===
        if (appointment.EmployeeID != dto.EmployeeID)
            return Forbid("You can only start your own appointments.");

        // === 4. Cannot start a completed appointment ===
        if (appointment.Status == "Completed")
            return BadRequest("Cannot start a completed appointment.");

        // === 5. Auto-set StartDate to NOW (rounded to date only) ===
        var now = DateTime.Now;
        appointment.StartDate = DateTime.Now;                   
        appointment.Time = new TimeSpan(now.Hour, now.Minute, now.Second);
        appointment.Status = "InProgress";

        try
        {
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Appointment started and approved.",
                appointmentId = id,
                startDate = appointment.StartDate.ToString("yyyy-MM-dd"),
                time = appointment.Time.ToString(@"hh\:mm"),
                status = "Approved",
                employeeId = dto.EmployeeID
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }


    [HttpGet("service")]
    public async Task<ActionResult<IEnumerable<ServiceAppointmentDetailsDto>>> GetServiceAppointments(
    [FromQuery] string? option = null,
    [FromQuery] string? status = null)
    {
        var validOptions = new[] { "Full", "Half", "Custom" };

        if (option != null && !validOptions.Contains(option, StringComparer.OrdinalIgnoreCase))
            return BadRequest("Invalid option. Use: Full, Half, Custom, or leave empty for all.");

        // === Base query: only Service appointments ===
        var query = _context.Appointments
            .Where(a => a.AppointmentType == "Service");

        // === FILTER BY STATUS (DB-side, exact match) ===
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(a => a.Status == status);
        }

        // === LOAD DATA (no case-insensitive string filter yet) ===
        var appointments = await query
            .Include(a => a.Customer).ThenInclude(c => c.User)
            .Include(a => a.Vehicle)
            .Include(a => a.Employee).ThenInclude(e => e.User)
            .Include(a => a.ServiceDetails)
                .ThenInclude(sd => sd.ServicePackage)
                .ThenInclude(p => p!.Items)
                .ThenInclude(i => i.Service)
            .Include(a => a.AppointmentServices)
                .ThenInclude(aps => aps.Service)
            .OrderByDescending(a => a.StartDate)
            .ThenBy(a => a.Time)
            .ToListAsync();

        // === APPLY CASE-INSENSITIVE OPTION FILTER IN MEMORY ===
        if (!string.IsNullOrWhiteSpace(option))
        {
            var opt = option.Trim();
            appointments = appointments
                .Where(a => a.ServiceDetails != null &&
                            string.Equals(a.ServiceDetails.ServiceOption, opt, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // === MAP TO DTO ===
        var result = appointments.Select(a => new ServiceAppointmentDetailsDto
        {
            AppointmentID = a.AppointmentID,
            CustomerName = a.Customer.User.UserName,
            CustomerEmail = a.Customer.User.Email ?? "N/A",
            VehicleInfo = $"{a.Vehicle.Company} {a.Vehicle.Model} ({a.Vehicle.Year}) - {a.Vehicle.PlateNumber}",
            StartDate = a.StartDate,
            Time = a.Time,
            Status = a.Status,
            EmployeeName = a.Employee?.User?.UserName ?? "Not Assigned",
            ServiceOption = a.ServiceDetails?.ServiceOption ?? "Unknown",
            TotalPrice = a.TotalPrice ?? 0m,

            PackageName = a.ServiceDetails?.ServicePackage?.Name,
            PackageType = a.ServiceDetails?.ServicePackage?.PackageType,
            PackageServices = a.ServiceDetails?.ServicePackage?.Items?
                .Select(i => new ServiceItemDto
                {
                    Title = i.Service.Title,
                    Price = i.Service.Price
                })
                .ToList() ?? new List<ServiceItemDto>(),

            CustomServices = a.AppointmentServices?
                .Select(aps => new ServiceItemDto
                {
                    Title = aps.Service.Title,
                    Price = aps.CustomPrice ?? aps.Service.Price
                })
                .ToList() ?? new List<ServiceItemDto>()
        }).ToList();

        return Ok(result);
    }

    [HttpGet("customer/{customerId}/vehicle/{vehicleId}/history")]
    public async Task<ActionResult<IEnumerable<ServiceHistoryItemDto>>> GetVehicleServiceHistory(
    string customerId,
    int vehicleId)
    {
        // === 1. Validate Vehicle Ownership ===
        var vehicleExists = await _context.Vehicles
            .AnyAsync(v => v.VehicleID == vehicleId && v.CustomerID == customerId);

        if (!vehicleExists)
            return NotFound("Vehicle not found or does not belong to the customer.");

        // === 2. Fetch Appointments (EF Core safe) ===
        var appointments = await _context.Appointments
            .Where(a => a.CustomerID == customerId &&
                        a.VehicleID == vehicleId &&
                        a.AppointmentType == "Service")
            .Include(a => a.ServiceDetails!)
                .ThenInclude(sd => sd!.ServicePackage!)
                .ThenInclude(p => p!.Items!)
                .ThenInclude(i => i!.Service)
            .Include(a => a.AppointmentServices!)
                .ThenInclude(aps => aps!.Service)
            .ToListAsync();

        var history = new List<ServiceHistoryItemDto>();

        foreach (var appt in appointments)
        {
            // === Determine EndDate or Message ===
            string endDateDisplay;
            if (appt.Status == "Completed")
            {
                endDateDisplay = (appt.EndDate ?? appt.StartDate.Add(appt.Time))
                    .ToString("yyyy-MM-dd HH:mm");
            }
            else
            {
                endDateDisplay = "Not completed yet";
            }

            // === FULL / HALF PACKAGE ===
            if (appt.ServiceDetails?.ServiceOption is "Full" or "Half")
            {
                if (appt.ServiceDetails.ServicePackage?.Items != null)
                {
                    foreach (var item in appt.ServiceDetails.ServicePackage.Items)
                    {
                        history.Add(new ServiceHistoryItemDto
                        {
                            Title = item.Service.Title,
                            Status = appt.Status,
                            Price = item.Service.Price,
                            EndDateDisplay = endDateDisplay
                        });
                    }
                }
            }
            // === CUSTOM SERVICES ===
            else if (appt.ServiceDetails?.ServiceOption == "Custom")
            {
                foreach (var aps in appt.AppointmentServices)
                {
                    history.Add(new ServiceHistoryItemDto
                    {
                        Title = aps.Service.Title,
                        Status = appt.Status,
                        Price = aps.CustomPrice ?? aps.Service.Price,
                        EndDateDisplay = endDateDisplay
                    });
                }
            }
        }

        // === 3. Sort: Pending first, then by EndDate (or default) ===
        var sortedHistory = history
            .OrderBy(h => h.Status == "Pending" ? 0 : 1)
            .ThenByDescending(h => h.EndDateDisplay == "Not completed yet" ? DateTime.MinValue :
                DateTime.ParseExact(h.EndDateDisplay, "yyyy-MM-dd HH:mm", null))
            .ToList();

        return Ok(sortedHistory);
    }

    // Helper: Count services in an appointment (ServicePackage or Custom)
    private int CountServicesInAppointment(Appointment appt)
    {
        if (appt.ServiceDetails?.ServicePackage != null)
        {
            return appt.ServiceDetails.ServicePackage.Items?.Count ?? 0;
        }
        else if (appt.AppointmentServices != null)
        {
            return appt.AppointmentServices.Count;
        }
        return 0;
    }

}
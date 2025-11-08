using System.ComponentModel.DataAnnotations;

namespace backend_EAD.DTOs;

// Main Profile Response DTO
public class CustomerProfileDto
{
    public UserInfoDto User { get; set; } = null!;
    public VehicleInfoDto? Vehicle { get; set; }
    public UpcomingAppointmentDto? UpcomingAppointment { get; set; }
    public List<ServiceHistoryDto> ServiceHistory { get; set; } = new();
    public List<ProjectInfoDto> Projects { get; set; } = new();
}

// User Information Section
public class UserInfoDto
{
    public string UserID { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Address { get; set; }
    public int LoyaltyPoints { get; set; }
    public string? Role { get; set; }
}

// Vehicle Information Section
public class VehicleInfoDto
{
    public int VehicleID { get; set; }
    public string PlateNumber { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string? Company { get; set; }
    public string Year { get; set; } = null!;
    public string Vin { get; set; } = null!;
}

// Upcoming Appointment Section
public class UpcomingAppointmentDto
{
    public int AppointmentID { get; set; }
    public string ServiceTitle { get; set; } = null!;
    public DateTime Date { get; set; }
    public string Time { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int? ProgressPercentage { get; set; }
    public string AppointmentType { get; set; } = null!; // Service or Project
    public string? EmployeeName { get; set; }
}

// Service History Section
public class ServiceHistoryDto
{
    public int AppointmentID { get; set; }
    public string ServiceTitle { get; set; } = null!;
    public DateTime Date { get; set; }
    public string Status { get; set; } = null!;
    public decimal? TotalPrice { get; set; }
    public string AppointmentType { get; set; } = null!;
}

// Project/Modification Request Section
public class ProjectInfoDto
{
    public int ProjectID { get; set; }
    public string ProjectTitle { get; set; } = null!;
    public string? ProjectDescription { get; set; }
    public string Status { get; set; } = null!;
    public DateTime RequestedAt { get; set; }
    public string? AssignedEmployee { get; set; }
    public decimal? TotalPrice { get; set; }
}

// Update Profile DTO
public class UpdateProfileDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }
}

// Update/Add Vehicle DTO
public class UpdateVehicleDto
{
    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = null!;

    [Required]
    [MaxLength(4)]
    public string Year { get; set; } = null!;

    [Required]
    [MaxLength(17)]
    public string Vin { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string PlateNumber { get; set; } = null!;

    [MaxLength(100)]
    public string? Company { get; set; }
}

// ============== EMPLOYEE PROFILE DTOs ==============

// Employee Profile Response DTO
public class EmployeeProfileDto
{
    public string EmployeeID { get; set; } = null!;
    public string UserID { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Position { get; set; } = null!;
    public decimal HourlyRate { get; set; }
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public List<EmployeeAppointmentDto> RecentAppointments { get; set; } = new();
}

// Employee Appointment DTO
public class EmployeeAppointmentDto
{
    public int AppointmentID { get; set; }
    public string CustomerName { get; set; } = null!;
    public string AppointmentType { get; set; } = null!; // Service or Project
    public string ServiceTitle { get; set; } = null!;
    public DateTime Date { get; set; }
    public string Time { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? VehicleInfo { get; set; }
}

// Update Employee Profile DTO
public class UpdateEmployeeProfileDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Position { get; set; }
}

// =====================================================
// Customer DTOs (for admin/internal use)
// =====================================================
public class CustomerDTO
{
    public string UserID { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
    public DateTime CreatedAt { get; set; }
    public int LoyaltyPoints { get; set; }
    public string? Address { get; set; }
}

/// <summary>
/// Update customer DTO - Only allows updating UserName, PhoneNumber, and Address
/// Email, LoyaltyPoints are restricted for security and business logic
/// </summary>
public class UpdateCustomerDTO
{
    public string? UserName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}

// =====================================================
// Vehicle DTOs (for public API)
// =====================================================

/// <summary>
/// Vehicle information DTO
/// </summary>
public class VehicleDto
{
    public int VehicleID { get; set; }
    public string Model { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public string? Company { get; set; }
}

/// <summary>
/// Add vehicle with CustomerID (for public API)
/// </summary>
public class AddVehicleWithCustomerDto
{
    public string CustomerID { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public string? Company { get; set; }
}

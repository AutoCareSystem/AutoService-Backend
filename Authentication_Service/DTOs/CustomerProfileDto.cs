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
    public int UserID { get; set; }
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

// DTOs/ServiceAppointmentDetailsDto.cs
namespace Service_Management_Service.DTOs;

public class ServiceAppointmentDetailsDto
{
    public int AppointmentID { get; set; }
    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string VehicleInfo { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public TimeSpan Time { get; set; }
    public string Status { get; set; } = null!;
    public string EmployeeName { get; set; } = null!;
    public string ServiceOption { get; set; } = null!;
    public decimal TotalPrice { get; set; }

    // Full / Half
    public string? PackageName { get; set; }
    public string? PackageType { get; set; }
    public List<ServiceItemDto> PackageServices { get; set; } = new();

    // Custom
    public List<ServiceItemDto> CustomServices { get; set; } = new();
}

// Reusable for both Package & Custom
public class ServiceItemDto
{
    public string Title { get; set; } = null!;
    public decimal Price { get; set; }
}
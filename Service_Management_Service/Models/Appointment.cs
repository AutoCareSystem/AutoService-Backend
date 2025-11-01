using System.ComponentModel.DataAnnotations;

namespace Service_Management_Service.Models;

public class Appointment
{
    [Key]
    public int AppointmentID { get; set; }

    public int CustomerID { get; set; }
    public Customer Customer { get; set; } = null!;

    public int VehicleID { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public TimeSpan Time { get; set; }
    public DateTime EndDate { get; set; }

    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Completed, Cancelled

    // Type of appointment
    public string AppointmentType { get; set; } = null!; // "Service" or "Project"

    // Service-specific fields
    public string? ServiceOption { get; set; } // "Full", "Half", "Custom" (only for Service type)

    // Project-specific fields
    public string? ProjectTitle { get; set; }
    public string? ProjectDescription { get; set; }

    // Navigation
    public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
}
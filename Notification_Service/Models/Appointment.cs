using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notification_Service.Models;

[Table("Appointments")]
public class Appointment
{
    [Key]
    public int AppointmentID { get; set; }

    [Required]
    public string CustomerID { get; set; } = null!;

    public int? VehicleID { get; set; }

    public string? EmployeeID { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public TimeSpan Time { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    [Required]
    [MaxLength(50)]
    public string AppointmentType { get; set; } = null!;

    [Column(TypeName = "decimal(10,2)")]
    public decimal? TotalPrice { get; set; }
}

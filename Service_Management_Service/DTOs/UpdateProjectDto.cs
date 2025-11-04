// DTOs/UpdateProjectDto.cs
using System.ComponentModel.DataAnnotations;

namespace Service_Management_Service.DTOs;

public class UpdateProjectDto
{
    [Required]
    public int AppointmentID { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public TimeSpan Time { get; set; }

    public DateTime? EndDate { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; } // Pending, InProgress, Completed, Cancelled

    [Required, MaxLength(200)]
    public string ProjectTitle { get; set; } = null!;

    public string? ProjectDescription { get; set; }

    public int? EmployeeID { get; set; }
}
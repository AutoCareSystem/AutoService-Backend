// DTOs/StartAppointmentDto.cs
using System.ComponentModel.DataAnnotations;

namespace Service_Management_Service.DTOs;

public class StartAppointmentDto
{
    [Required]
    public string EmployeeID { get; set; } = null!;
}
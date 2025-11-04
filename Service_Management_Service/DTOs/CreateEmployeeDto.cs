// DTOs/CreateEmployeeDto.cs
using System.ComponentModel.DataAnnotations;

namespace Service_Management_Service.DTOs;

public class CreateEmployeeDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = null!;

    [Required, Phone, MaxLength(20)]
    public string Phone { get; set; } = null!;

    [Required, MaxLength(50)]
    public string EmpNo { get; set; } = null!; // e.g., "EMP001"

    [Required, MaxLength(50)]
    public string Position { get; set; } = null!; // e.g., "Mechanic", "Manager"
}
// DTOs/CreateServicePackageDto.cs
using System.ComponentModel.DataAnnotations;

namespace Service_Management_Service.DTOs;

public class CreateServicePackageDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required, MaxLength(50)]
    public string? PackageType { get; set; } // Optional, free-form

    [Required, MinLength(1)]
    public List<int> ServiceIDs { get; set; } = null!;
}
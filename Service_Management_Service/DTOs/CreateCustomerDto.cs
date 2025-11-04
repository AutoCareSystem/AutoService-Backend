using System.ComponentModel.DataAnnotations;

namespace Service_Management_Service.DTOs;

public class CreateCustomerDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = null!;

    [Required, Phone, MaxLength(20)]
    public string Phone { get; set; } = null!;

    [MaxLength(200)]
    public string? Address { get; set; }
}
// Models/User.cs
using System.ComponentModel.DataAnnotations;

namespace Service_Management_Service.Models;

public class User
{
    [Key]
    public int UserID { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Email { get; set; } = null!;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = null!;

    [Required, MaxLength(20)]
    public string Role { get; set; } = null!; // "Customer", "Employee", "Admin"
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_EAD.Models;

[Table("Employees")]
public class Employee
{
    [Key, ForeignKey(nameof(AppUser))]
    public string AppUserId { get; set; } = null!; // Changed from int UserID to string AppUserId

    public AppUser AppUser { get; set; } = null!; // Changed from User to AppUser

    [Required, MaxLength(100)]
    public string Position { get; set; } = null!;

    public decimal HourlyRate { get; set; }

    // Navigations
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

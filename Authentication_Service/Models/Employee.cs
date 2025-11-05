using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_EAD.Models;

[Table("Employees")]
public class Employee
{
    [Key, ForeignKey(nameof(User))]
    public int UserID { get; set; }

    public User User { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Position { get; set; } = null!;

    public decimal HourlyRate { get; set; }

    // Navigations
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

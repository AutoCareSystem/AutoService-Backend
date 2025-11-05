using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Management_Service.Models;

[Table("Employees")]
public class Employee
{
    [Key, ForeignKey(nameof(User))]
    public int UserID { get; set; }

    public User User { get; set; } = null!;

    [Required, MaxLength(50)]
    public string EmpNo { get; set; } = null!;

    [Required, MaxLength(50)]
    public string Position { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public ICollection<Appointment> AssignedAppointments { get; set; } = new List<Appointment>();
}
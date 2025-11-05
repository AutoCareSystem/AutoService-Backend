using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_EAD.Models;

[Table("ProjectAppointments")]
public class ProjectAppointment
{
    [Key, ForeignKey(nameof(Appointment))]
    public int AppointmentID { get; set; }

    [Required, MaxLength(200)]
    public string ProjectTitle { get; set; } = null!;

    public string? ProjectDescription { get; set; }

    public Appointment Appointment { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_EAD.Models;

[Table("AppointmentServices")]
public class AppointmentService
{
    [Key]
    public int AppointmentServiceID { get; set; }

    public int AppointmentID { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public int ServiceID { get; set; }
    public Service Service { get; set; } = null!;

    public decimal? Price { get; set; }
    public int? Duration { get; set; }
}

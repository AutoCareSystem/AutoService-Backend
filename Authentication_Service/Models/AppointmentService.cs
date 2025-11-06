using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_EAD.Models;

public class AppointmentService
{
    [Key]
    public int AppointmentServiceID { get; set; }

    public int AppointmentID { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public int ServiceID { get; set; }
    public Service Service { get; set; } = null!;

    // Optional override (e.g. custom price)
    public decimal? CustomPrice { get; set; }
}

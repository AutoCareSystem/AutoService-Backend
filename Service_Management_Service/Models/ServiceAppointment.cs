using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Management_Service.Models;

[Table("ServiceAppointments")]
public class ServiceAppointment
{
    [Key, ForeignKey(nameof(Appointment))]
    public int AppointmentID { get; set; }

    [Required]
    public string ServiceOption { get; set; } = null!; // Full, Half, Custom

    public int? ServicePackageID { get; set; }
    public ServicePackage? ServicePackage { get; set; }

    public Appointment Appointment { get; set; } = null!;
}
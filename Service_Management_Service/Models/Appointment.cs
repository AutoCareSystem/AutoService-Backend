using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Service_Management_Service.Models;

namespace Service_Management_Service.Models
{
    [Table("Appointments")]
    public class Appointment
    {
        [Key]
        public int AppointmentID { get; set; }

        public string CustomerID { get; set; } = null!;
        public Customer Customer { get; set; } = null!;

        public int VehicleID { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        public string? EmployeeID { get; set; }
        public Employee? Employee { get; set; }

        public DateTime StartDate { get; set; }

        public TimeSpan Time { get; set; }

        public DateTime? EndDate { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Confirmed, In Progress, Completed, Cancelled

        public string AppointmentType { get; set; } = null!; // "Service" or "Project"

        public decimal? TotalPrice { get; set; }

        // Composition
        public ServiceAppointment? ServiceDetails { get; set; }
        public ProjectAppointment? ProjectDetails { get; set; }

        public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
    }
}
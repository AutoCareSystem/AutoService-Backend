using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Service_Management_Service.Models
{
    [Table("Employees")]
    public class Employee
    {
        [Key, ForeignKey(nameof(User))]
        public string UserID { get; set; } = null!;  // Now string to match AppUser.Id

        public AppUser User { get; set; } = null!;

        [Required, MaxLength(50)]
        public string EmpNo { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Position { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        // Navigations
        public ICollection<Appointment> AssignedAppointments { get; set; } = new List<Appointment>();
    }
}
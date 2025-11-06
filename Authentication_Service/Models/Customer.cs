using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace backend_EAD.Models
{
    [Table("Customers")]
    public class Customer
    {
        [Key, ForeignKey(nameof(User))]
        public string UserID { get; set; } = null!;  // Now string to match AppUser.Id

        public AppUser User { get; set; } = null!;

        public int LoyaltyPoints { get; set; } = 0;

        [MaxLength(200)]
        public string? Address { get; set; }

        // Navigations
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
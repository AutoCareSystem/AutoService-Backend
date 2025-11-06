using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_EAD.Models;

[Table("Customers")]
public class Customer
{
    [Key, ForeignKey(nameof(AppUser))]
    public string AppUserId { get; set; } = null!; // Changed from int UserID to string AppUserId

    public AppUser AppUser { get; set; } = null!; // Changed from User to AppUser

    public int LoyaltyPoints { get; set; } = 0;

    [MaxLength(200)]
    public string? Address { get; set; }

    // Navigations
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

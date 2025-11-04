using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Service_Management_Service.Models;

namespace Service_Management_Service.Models;

[Table("Customers")]
public class Customer
{
    [Key, ForeignKey(nameof(User))]
    public int UserID { get; set; }

    public User User { get; set; } = null!;

    public int LoyaltyPoints { get; set; } = 0;

    [MaxLength(200)]
    public string? Address { get; set; }

    // Navigations
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
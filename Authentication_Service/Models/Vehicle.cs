using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_EAD.Models;

[Table("Vehicles")]
public class Vehicle
{
    [Key]
    public int VehicleID { get; set; }

    [ForeignKey(nameof(Customer))]
    public string CustomerID { get; set; } = null!; // Changed from int to string to match Customer.AppUserId

    public Customer Customer { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Model { get; set; } = null!;

    [Required, MaxLength(4)]
    public string Year { get; set; } = null!;

    [Required, MaxLength(17)]
    public string Vin { get; set; } = null!;

    [Required, MaxLength(20)]
    public string PlateNumber { get; set; } = null!;

    [MaxLength(100)]
    public string? Company { get; set; }
}

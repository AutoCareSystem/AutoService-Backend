using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_EAD.Models;

[Table("Vehicles")]
public class Vehicle
{
    [Key]
    public int VehicleID { get; set; }

    public int CustomerID { get; set; }
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

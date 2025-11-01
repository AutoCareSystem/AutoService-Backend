using System.ComponentModel.DataAnnotations;

namespace Service_Management_Service.Models;

public class Vehicle
{
    public int VehicleID { get; set; }
    public int CustomerID { get; set; }
    public Customer Customer { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string Year { get; set; } = null!;
    public string Vin { get; set; } = null!;
    public string PlateNumber { get; set; } = null!;
    public string? Company { get; set; }
}
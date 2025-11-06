using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Management_Service.Models
{
    public class Vehicle
    {
        [Key]
        public int VehicleID { get; set; }
        public string CustomerID { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Year { get; set; } = null!;
        public string Vin { get; set; } = null!;
        public string PlateNumber { get; set; } = null!;
        public string? Company { get; set; }
    }
}
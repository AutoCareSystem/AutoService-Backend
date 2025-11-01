using System.ComponentModel.DataAnnotations;

namespace Service_Management_Service.Models;
 
public class Service
{
    [Key]
    public int ServiceID { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public int Duration { get; set; } // in minutes or hours

    [Required]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Active";
}

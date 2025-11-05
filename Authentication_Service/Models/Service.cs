using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_EAD.Models;

[Table("Services")]
public class Service
{
    [Key]
    public int ServiceID { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public int Duration { get; set; } // in minutes

    [Required]
    public decimal Price { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Active";
}

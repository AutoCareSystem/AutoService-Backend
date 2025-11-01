using System.ComponentModel.DataAnnotations;

namespace Service_Management_Service.Models;

public class ServicePackage
{
    [Key]
    public int ServicePackageID { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    // This is the key!
    [Required]
    public string PackageType { get; set; } = null!; // "Full" or "Half"

    public ICollection<ServicePackageItem> Items { get; set; } = new List<ServicePackageItem>();

}

public class ServicePackageItem
{
    [Key]
    public int ServicePackageItemID { get; set; }

    public int ServicePackageID { get; set; }
    public ServicePackage Package { get; set; } = null!;

    public int ServiceID { get; set; }
    public Service Service { get; set; } = null!;
}
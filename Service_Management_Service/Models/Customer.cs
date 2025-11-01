// Models/Customer.cs
using System.ComponentModel.DataAnnotations;

namespace Service_Management_Service.Models;

public class Customer : User
{
    public int CustomerID { get; set; }  // Same as UserID (EF maps it)

    public int LoyaltyPoints { get; set; } = 0;

    [MaxLength(200)]
    public string? Address { get; set; }
}
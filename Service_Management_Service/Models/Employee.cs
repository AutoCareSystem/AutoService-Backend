// Models/Employee.cs
using System.ComponentModel.DataAnnotations;

namespace Service_Management_Service.Models;

public class Employee : User
{
    public int EmpID { get; set; }  // Same as UserID

    [Required, MaxLength(50)]
    public string EmpNo { get; set; } = null!; // e.g. "EMP001"

    [Required, MaxLength(50)]
    public string Position { get; set; } = null!; // "Mechanic", "Manager"

    public bool IsActive { get; set; } = true;
}
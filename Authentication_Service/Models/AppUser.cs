using Microsoft.AspNetCore.Identity;

namespace backend_EAD.Models
{
    public class AppUser : IdentityUser
    {
        // Additional fields from old Users table
        public string Name { get; set; } = null!; // Maps to old Users.Name
        public string Role { get; set; } = "Customer"; // Customer, Employee, Admin
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Customer? Customer { get; set; }
        public Employee? Employee { get; set; }
    }
}

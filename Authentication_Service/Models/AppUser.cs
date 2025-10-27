using Microsoft.AspNetCore.Identity;

namespace backend_EAD.Models
{
    public class AppUser : IdentityUser
    {
        public string Role { get; set; } = "User"; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

using Microsoft.AspNetCore.Identity;

namespace Service_Management_Service.Models
{
	public class AppUser : IdentityUser
	{
		public string Role { get; set; } = "User";
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
using Microsoft.EntityFrameworkCore;
using Service_Management_Service.Models;

namespace Service_Management_Service.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<Service> Services { get; set; } 
	}
}

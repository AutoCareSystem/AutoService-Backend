using Microsoft.EntityFrameworkCore;
using Service_Management_Service.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Service> Services { get; set; }
}

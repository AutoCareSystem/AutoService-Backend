using Microsoft.EntityFrameworkCore;
using Service_Management_Service.Models;

namespace Service_Management_Service.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<ServicePackage> ServicePackages { get; set; } = null!;
        public DbSet<ServicePackageItem> ServicePackageItems { get; set; } = null!;
        public DbSet<AppointmentService> AppointmentServices { get; set; } = null!;
        public DbSet<ServiceAppointment> ServiceAppointments { get; set; } = null!;
        public DbSet<ProjectAppointment> ProjectAppointments { get; set; } = null!;

    }
}

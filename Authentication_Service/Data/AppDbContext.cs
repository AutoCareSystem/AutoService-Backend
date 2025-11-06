using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using backend_EAD.Models;

namespace AutoServiceBackend.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Authentication tables
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        // Service Management tables (shared database)
        // REMOVED: public DbSet<User> ServiceUsers { get; set; } = null!;  // Users table is now merged with AspNetUsers
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<ServiceAppointment> ServiceAppointments { get; set; } = null!;
        public DbSet<ProjectAppointment> ProjectAppointments { get; set; } = null!;
        public DbSet<AppointmentService> AppointmentServices { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Customer relationship with AppUser
            builder.Entity<Customer>()
                .HasOne(c => c.AppUser)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Employee relationship with AppUser
            builder.Entity<Employee>()
                .HasOne(e => e.AppUser)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Vehicle-Customer relationship
            builder.Entity<Vehicle>()
                .HasOne(v => v.Customer)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CustomerID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Appointment relationships
            builder.Entity<Appointment>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Appointments)
                .HasForeignKey(a => a.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Appointments)
                .HasForeignKey(a => a.EmployeeID)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

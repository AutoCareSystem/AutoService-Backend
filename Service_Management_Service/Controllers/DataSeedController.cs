using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Management_Service.Data;
using Service_Management_Service.Models;

namespace Service_Management_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DataSeedController : ControllerBase
{
    private readonly AppDbContext _context;

    public DataSeedController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("seed-sample-data")]
    public async Task<IActionResult> SeedSampleData()
    {
        try
        {
            // Check if data already exists
            var existingCustomers = await _context.Customers.CountAsync();
            if (existingCustomers > 0)
            {
                return Ok("Sample data already exists.");
            }

            // Create sample customers
            var customers = new List<Customer>
            {
                new Customer
                {
                    UserID = "f003b7d9-eefe-4cb6-8f87-06ff62c54d8a",
                    User = new AppUser
                    {
                        Id = "f003b7d9-eefe-4cb6-8f87-06ff62c54d8a",
                        UserName = "john.doe@example.com",
                        Email = "john.doe@example.com",
                        PhoneNumber = "+1-555-0123",
                        Role = "Customer"
                    },
                    Address = "123 Main Street, Anytown, AT 12345",
                    LoyaltyPoints = 150
                },
                new Customer
                {
                    UserID = "a004c8e0-ffff-5dc7-9g98-17gg73d65e9b",
                    User = new AppUser
                    {
                        Id = "a004c8e0-ffff-5dc7-9g98-17gg73d65e9b",
                        UserName = "jane.smith@example.com",
                        Email = "jane.smith@example.com",
                        PhoneNumber = "+1-555-0456",
                        Role = "Customer"
                    },
                    Address = "456 Oak Avenue, Somewhere, SW 67890",
                    LoyaltyPoints = 75
                }
            };

            _context.Customers.AddRange(customers);

            // Create sample employees
            var employees = new List<Employee>
            {
                new Employee
                {
                    UserID = "e001b7d9-eefe-4cb6-8f87-06ff62c54d8a",
                    User = new AppUser
                    {
                        Id = "e001b7d9-eefe-4cb6-8f87-06ff62c54d8a",
                        UserName = "mike.mechanic@autocare.com",
                        Email = "mike.mechanic@autocare.com",
                        PhoneNumber = "+1-555-7890",
                        Role = "Employee"
                    },
                    Position = "Senior Technician",
                    IsActive = true
                },
                new Employee
                {
                    UserID = "e002c8e0-ffff-5dc7-9g98-17gg73d65e9b",
                    User = new AppUser
                    {
                        Id = "e002c8e0-ffff-5dc7-9g98-17gg73d65e9b",
                        UserName = "sarah.tech@autocare.com",
                        Email = "sarah.tech@autocare.com",
                        PhoneNumber = "+1-555-4567",
                        Role = "Employee"
                    },
                    Position = "Automotive Specialist",
                    IsActive = true
                }
            };

            _context.Employees.AddRange(employees);

            // Create sample vehicles
            var vehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    CustomerID = "f003b7d9-eefe-4cb6-8f87-06ff62c54d8a",
                    Company = "Honda",
                    Model = "Civic",
                    Year = "2020",
                    PlateNumber = "ABC-1234",
                    Vin = "1HGCV1F30LA123456"
                },
                new Vehicle
                {
                    CustomerID = "f003b7d9-eefe-4cb6-8f87-06ff62c54d8a",
                    Company = "Toyota",
                    Model = "Camry",
                    Year = "2019",
                    PlateNumber = "XYZ-5678",
                    Vin = "4T1BF1FK5KU123456"
                },
                new Vehicle
                {
                    CustomerID = "a004c8e0-ffff-5dc7-9g98-17gg73d65e9b",
                    Company = "BMW",
                    Model = "X3",
                    Year = "2021",
                    PlateNumber = "BMW-9012",
                    Vin = "5UX2V2C59M9123456"
                }
            };

            _context.Vehicles.AddRange(vehicles);

            // Create sample services
            var services = new List<Service>
            {
                new Service
                {
                    Code = "OIL001",
                    Title = "Engine Oil Change",
                    Description = "Complete engine oil and filter replacement",
                    Price = 75.00m,
                    Duration = 60, // 60 minutes
                    Status = "Active"
                },
                new Service
                {
                    Code = "BRK001",
                    Title = "Brake Pad Replacement",
                    Description = "Front and rear brake pad replacement",
                    Price = 250.00m,
                    Duration = 120, // 120 minutes
                    Status = "Active"
                },
                new Service
                {
                    Code = "TIR001",
                    Title = "Tire Rotation",
                    Description = "Complete tire rotation and balancing",
                    Price = 50.00m,
                    Duration = 45, // 45 minutes
                    Status = "Active"
                },
                new Service
                {
                    Code = "DIG001",
                    Title = "Engine Diagnostic",
                    Description = "Computer diagnostic scan and analysis",
                    Price = 120.00m,
                    Duration = 60, // 60 minutes
                    Status = "Active"
                },
                new Service
                {
                    Code = "AIR001",
                    Title = "Air Filter Replacement",
                    Description = "Engine and cabin air filter replacement",
                    Price = 65.00m,
                    Duration = 30, // 30 minutes
                    Status = "Active"
                }
            };

            _context.Services.AddRange(services);

            // Save first to get IDs
            await _context.SaveChangesAsync();

            // Create sample service packages
            var servicePackages = new List<ServicePackage>
            {
                new ServicePackage
                {
                    Name = "Basic Maintenance Package",
                    Description = "Essential maintenance services",
                    PackageType = "Half",
                    Price = 150.00m,
                    Items = new List<ServicePackageItem>
                    {
                        new ServicePackageItem { ServiceID = services[0].ServiceID },
                        new ServicePackageItem { ServiceID = services[4].ServiceID }
                    }
                },
                new ServicePackage
                {
                    Name = "Complete Service Package",
                    Description = "Comprehensive vehicle service",
                    PackageType = "Full",
                    Price = 450.00m,
                    Items = new List<ServicePackageItem>
                    {
                        new ServicePackageItem { ServiceID = services[0].ServiceID },
                        new ServicePackageItem { ServiceID = services[1].ServiceID },
                        new ServicePackageItem { ServiceID = services[2].ServiceID },
                        new ServicePackageItem { ServiceID = services[3].ServiceID },
                        new ServicePackageItem { ServiceID = services[4].ServiceID }
                    }
                }
            };

            _context.ServicePackages.AddRange(servicePackages);
            await _context.SaveChangesAsync();

            // Create sample appointments
            var appointments = new List<Appointment>
            {
                // Completed appointment
                new Appointment
                {
                    CustomerID = "f003b7d9-eefe-4cb6-8f87-06ff62c54d8a",
                    VehicleID = vehicles[0].VehicleID,
                    EmployeeID = "e001b7d9-eefe-4cb6-8f87-06ff62c54d8a",
                    StartDate = DateTime.Now.AddDays(-7),
                    Time = TimeSpan.FromHours(10),
                    EndDate = DateTime.Now.AddDays(-7).AddHours(2),
                    Status = "Completed",
                    AppointmentType = "Service",
                    TotalPrice = 150.00m,
                    ServiceDetails = new ServiceAppointment
                    {
                        ServiceOption = "Half",
                        ServicePackageID = servicePackages[0].ServicePackageID
                    }
                },
                // In Progress appointment
                new Appointment
                {
                    CustomerID = "f003b7d9-eefe-4cb6-8f87-06ff62c54d8a",
                    VehicleID = vehicles[1].VehicleID,
                    EmployeeID = "e002c8e0-ffff-5dc7-9g98-17gg73d65e9b",
                    StartDate = DateTime.Now,
                    Time = TimeSpan.FromHours(9),
                    Status = "In Progress",
                    AppointmentType = "Service",
                    TotalPrice = 450.00m,
                    ServiceDetails = new ServiceAppointment
                    {
                        ServiceOption = "Full",
                        ServicePackageID = servicePackages[1].ServicePackageID
                    }
                },
                // Pending appointment
                new Appointment
                {
                    CustomerID = "f003b7d9-eefe-4cb6-8f87-06ff62c54d8a",
                    VehicleID = vehicles[0].VehicleID,
                    StartDate = DateTime.Now.AddDays(3),
                    Time = TimeSpan.FromHours(14),
                    Status = "Pending",
                    AppointmentType = "Service",
                    TotalPrice = 250.00m,
                    ServiceDetails = new ServiceAppointment
                    {
                        ServiceOption = "Custom"
                    }
                },
                // Custom project
                new Appointment
                {
                    CustomerID = "f003b7d9-eefe-4cb6-8f87-06ff62c54d8a",
                    VehicleID = vehicles[1].VehicleID,
                    StartDate = DateTime.Now.AddDays(5),
                    Time = TimeSpan.FromHours(8),
                    Status = "Approved",
                    AppointmentType = "Project",
                    TotalPrice = 2500.00m,
                    ProjectDetails = new ProjectAppointment
                    {
                        ProjectTitle = "Custom Paint Job",
                        ProjectDescription = "Complete vehicle repaint with custom color scheme"
                    }
                }
            };

            _context.Appointments.AddRange(appointments);

            // Add appointment services for custom appointment
            var customAppointment = appointments[2];
            customAppointment.AppointmentServices = new List<AppointmentService>
            {
                new AppointmentService
                {
                    ServiceID = services[1].ServiceID,
                    CustomPrice = 250.00m
                }
            };

            await _context.SaveChangesAsync();

            return Ok("Sample data seeded successfully!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error seeding data: {ex.Message}");
        }
    }

    [HttpGet("debug/data")]
    public async Task<IActionResult> GetDebugData()
    {
        try
        {
            var customers = await _context.Customers
                .Include(c => c.User)
                .Select(c => new
                {
                    c.UserID,
                    c.User.UserName,
                    c.User.Email,
                    c.Address
                })
                .ToListAsync();

            var vehicles = await _context.Vehicles
                .Select(v => new
                {
                    v.VehicleID,
                    v.CustomerID,
                    v.Company,
                    v.Model,
                    v.Year,
                    v.PlateNumber,
                    v.Vin
                })
                .ToListAsync();

            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                    .ThenInclude(c => c.User)
                .Include(a => a.Vehicle)
                .Include(a => a.Employee)
                    .ThenInclude(e => e.User)
                .Select(a => new
                {
                    a.AppointmentID,
                    CustomerName = a.Customer.User.UserName,
                    Vehicle = $"{a.Vehicle.Company} {a.Vehicle.Model}",
                    a.Status,
                    a.AppointmentType,
                    a.TotalPrice,
                    a.StartDate,
                    EmployeeName = a.Employee != null ? a.Employee.User.UserName : "Unassigned"
                })
                .ToListAsync();

            return Ok(new
            {
                Customers = customers,
                Vehicles = vehicles,
                Appointments = appointments,
                Summary = new
                {
                    TotalCustomers = customers.Count,
                    TotalVehicles = vehicles.Count,
                    TotalAppointments = appointments.Count
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving debug data: {ex.Message}");
        }
    }

    [HttpDelete("clear-data")]
    public async Task<IActionResult> ClearAllData()
    {
        try
        {
            // Remove in correct order to handle foreign key constraints
            _context.AppointmentServices.RemoveRange(_context.AppointmentServices);
            _context.ServiceAppointments.RemoveRange(_context.ServiceAppointments);
            _context.ProjectAppointments.RemoveRange(_context.ProjectAppointments);
            _context.Appointments.RemoveRange(_context.Appointments);
            
            _context.ServicePackageItems.RemoveRange(_context.ServicePackageItems);
            _context.ServicePackages.RemoveRange(_context.ServicePackages);
            _context.Services.RemoveRange(_context.Services);
            
            _context.Vehicles.RemoveRange(_context.Vehicles);
            _context.Employees.RemoveRange(_context.Employees);
            _context.Customers.RemoveRange(_context.Customers);
            _context.Users.RemoveRange(_context.Users);

            await _context.SaveChangesAsync();
            return Ok("All data cleared successfully!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error clearing data: {ex.Message}");
        }
    }
}
// Controllers/ServicePackagesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Management_Service.Data;
using Service_Management_Service.Models;
using Service_Management_Service.DTOs;

namespace Service_Management_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServicePackagesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ServicePackagesController(AppDbContext context)
    {
        _context = context;
    }

    // POST: api/servicepackages
    [HttpPost]
    public async Task<ActionResult<ServicePackage>> CreateServicePackage([FromBody] CreateServicePackageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // === 1. Validate Name uniqueness ===
        if (await _context.ServicePackages.AnyAsync(p => p.Name == dto.Name))
            return Conflict($"Package name '{dto.Name}' already exists.");

        // === 2. Validate Service IDs ===
        if (dto.ServiceIDs == null || !dto.ServiceIDs.Any())
            return BadRequest("At least one service must be selected.");

        var uniqueServiceIds = dto.ServiceIDs.Distinct().ToList();
        if (uniqueServiceIds.Count != dto.ServiceIDs.Count)
            return BadRequest("Duplicate services are not allowed.");

        // === 3. Fetch services with Price ===
        var services = await _context.Services
            .Where(s => uniqueServiceIds.Contains(s.ServiceID) && s.Status == "Active")
            .Select(s => new { s.ServiceID, s.Price })
            .ToListAsync();

        if (services.Count != uniqueServiceIds.Count)
            return BadRequest("One or more services are invalid or inactive.");

        // === 4. Calculate total price ===
        decimal calculatedPrice = services.Sum(s => s.Price);

        // === 5. Create Package ===
        var package = new ServicePackage
        {
            Name = dto.Name,
            Description = dto.Description,
            PackageType = dto.PackageType?.Trim(),
            Price = calculatedPrice  // ← AUTO-CALCULATED
        };

        // === 6. Add Items ===
        foreach (var service in services)
        {
            package.Items.Add(new ServicePackageItem
            {
                ServiceID = service.ServiceID
            });
        }

        try
        {
            _context.ServicePackages.Add(package);
            await _context.SaveChangesAsync();

            // Load full data
            await _context.Entry(package)
                .Collection(p => p.Items)
                .Query()
                .Include(i => i.Service)
                .LoadAsync();

            return CreatedAtAction(
                nameof(GetServicePackage),
                new { id = package.ServicePackageID },
                package);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    // GET: api/servicepackages/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ServicePackage>> GetServicePackage(int id)
    {
        var package = await _context.ServicePackages
            .Include(p => p.Items)
                .ThenInclude(i => i.Service)
            .FirstOrDefaultAsync(p => p.ServicePackageID == id);

        if (package == null)
            return NotFound();

        return Ok(package);
    }

    // GET: api/servicepackages
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServicePackage>>> GetAllPackages()
    {
        var packages = await _context.ServicePackages
            .Include(p => p.Items)
                .ThenInclude(i => i.Service)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return Ok(packages);
    }
}
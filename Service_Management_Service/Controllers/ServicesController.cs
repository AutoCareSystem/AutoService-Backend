using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Management_Service.Data;
using Service_Management_Service.Models;

namespace Service_Management_Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ServicesController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/services
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Service>>> GetServices()
    {
        return await _db.Services.ToListAsync();
    }

    // GET: api/services/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Service>> GetService(int id)
    {
        var service = await _db.Services.FindAsync(id);

        if (service == null)
            return NotFound();

        return service;
    }

    // POST: api/services
    [HttpPost]
    public async Task<ActionResult<Service>> CreateService(Service service)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        _db.Services.Add(service);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetService), new { id = service.ServiceID }, service);
    }

    // PUT: api/services/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateService(int id, Service service)
    {
        if (id != service.ServiceID)
            return BadRequest("ID in route must match ID in body.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var existing = await _db.Services.FindAsync(id);
        if (existing == null)
            return NotFound();

        existing.Code = service.Code;
        existing.Title = service.Title;
        existing.Description = service.Description;
        existing.Duration = service.Duration;
        existing.Price = service.Price;
        existing.Status = service.Status;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ServiceExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    // DELETE: api/services/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        var service = await _db.Services.FindAsync(id);
        if (service == null)
            return NotFound();

        _db.Services.Remove(service);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private bool ServiceExists(int id) =>
        _db.Services.Any(e => e.ServiceID == id);
}
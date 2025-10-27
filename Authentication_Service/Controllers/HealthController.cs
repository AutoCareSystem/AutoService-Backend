using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoServiceBackend.Data;

namespace backend_EAD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HealthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Try to connect to database
                var canConnect = await _context.Database.CanConnectAsync();

                if (canConnect)
                {
                    return Ok(new
                    {
                        status = "healthy",
                        database = "connected",
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return StatusCode(503, new
                    {
                        status = "unhealthy",
                        database = "disconnected",
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    database = "error",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("db")]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                // More detailed database check
                var canConnect = await _context.Database.CanConnectAsync();

                if (canConnect)
                {
                    // Try to execute a simple query
                    var result = await _context.Database.ExecuteSqlRawAsync("SELECT 1");

                    return Ok(new
                    {
                        database = "connected",
                        queryTest = "passed",
                        connectionString = _context.Database.GetConnectionString()?.Substring(0, 50) + "...",
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return StatusCode(503, new
                    {
                        database = "disconnected",
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    database = "error",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
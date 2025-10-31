using Microsoft.EntityFrameworkCore;
using Service_Management_Service;

using Service_Management_Service.Models;

var builder = WebApplication.CreateBuilder(args);

// Get the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register ApplicationDbContext with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Minimal endpoint to get all services
app.MapGet("/services", async (ApplicationDbContext db) =>
{
    return await db.Services.ToListAsync();
});

app.Run();

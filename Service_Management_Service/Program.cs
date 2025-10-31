using Microsoft.EntityFrameworkCore;
using Service_Management_Service.Data; 

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)); 

var app = builder.Build();

app.UseHttpsRedirection();

app.Run();

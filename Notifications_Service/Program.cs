using Notifications_Service.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add SignalR
builder.Services.AddSignalR();

// Configure CORS to allow frontend connections
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5093", "http://localhost:5000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for SignalR
    });
});

var app = builder.Build();

// Use CORS before other middleware
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Map SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");

Console.WriteLine("🚀 Notifications_Service started successfully!");
Console.WriteLine("📡 SignalR Hub available at: /notificationHub");
Console.WriteLine("📍 API available at: http://localhost:5001");
Console.WriteLine("✅ Health Check: http://localhost:5001/api/notification/health");

app.Run();


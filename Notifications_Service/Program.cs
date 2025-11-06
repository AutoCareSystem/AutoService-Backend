using Notifications_Service.Hubs;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5093", "http://localhost:5000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); 
    });
});

var app = builder.Build();

// Use CORS before other middleware
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

Console.WriteLine("Notifications_Service started successfully!");
Console.WriteLine("SignalR Hub available at: /notificationHub");
Console.WriteLine("API available at: http://localhost:5001");
Console.WriteLine("Health Check: http://localhost:5001/api/notification/health");

app.Run();


using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Notification_Service.Data;
using Notification_Service.Hubs;
using Notification_Service.Services;

Env.Load();

// Configure PostgreSQL to handle DateTime properly
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();

// Get connection string
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("DATABASE_URL is missing. Set it in .env file.");
}

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    });
});

// Register Services
builder.Services.AddScoped<NotificationService>();
builder.Services.AddHostedService<AppointmentMonitorService>();

// Add SignalR
builder.Services.AddSignalR();

// Add Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Notification Service API",
        Version = "v1",
        Description = "Real-time notification service with SignalR for appointment and service updates"
    });
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification API V1");
    });
}

// Apply migrations and test database connection
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        Console.WriteLine($"Attempting to connect to database...");
        Console.WriteLine($"Connection String: {connectionString}");

        if (await dbContext.Database.CanConnectAsync())
        {
            Console.WriteLine("✅ Database connected successfully!");

            // Create database schema if it doesn't exist
            Console.WriteLine("🔄 Ensuring database schema exists...");
            await dbContext.Database.EnsureCreatedAsync();
            Console.WriteLine("✅ Database schema ready!");

            // Test if Notifications table exists
            try
            {
                var notificationCount = await dbContext.Notifications.CountAsync();
                Console.WriteLine($"📊 Total notifications in database: {notificationCount}");
            }
            catch
            {
                Console.WriteLine("⚠️ Notifications table check failed");
            }
        }
        else
        {
            Console.WriteLine("⚠️ Database connection failed - CanConnectAsync returned false!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database connection exception: {ex.Message}");
        Console.WriteLine($"Exception Type: {ex.GetType().Name}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        }
    }
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.MapControllers();

// Map SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");

Console.WriteLine("🚀 Notification Service with SignalR is starting...");
Console.WriteLine("📡 SignalR Hub available at: /notificationHub");

app.Run();

using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Chatbot_Service.Data;
using Chatbot_Service.Services;

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
builder.Services.AddScoped<TimeSlotService>();
builder.Services.AddScoped<GeminiService>();

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
        Title = "Chatbot Service API",
        Version = "v1",
        Description = "AI-powered chatbot for checking available appointment time slots"
    });
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chatbot API V1");
    });
}

// Test database connection
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

            // Test if Appointments table exists
            var appointmentCount = await dbContext.Appointments.CountAsync();
            Console.WriteLine($"📊 Total appointments in database: {appointmentCount}");
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

app.Run();

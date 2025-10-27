using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using AutoServiceBackend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using backend_EAD.Models;

var builder = WebApplication.CreateBuilder(args);

// -----------------------
// Load .env (for local development)
DotNetEnv.Env.Load();

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();

// -----------------------
// Read environment variables
var dbUrl = builder.Configuration["DATABASE_URL"];
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];



Console.WriteLine($"JWT_KEY length: {Environment.GetEnvironmentVariable("JWT_KEY")?.Length}");


// -----------------------
// Null checks
if (string.IsNullOrEmpty(dbUrl))
    throw new InvalidOperationException("DATABASE_URL is missing. Set it in .env or environment variables.");
if (string.IsNullOrEmpty(jwtKey))
    throw new InvalidOperationException("JWT_KEY is missing. Set it in .env or environment variables.");
if (string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
    throw new InvalidOperationException("JWT_ISSUER or JWT_AUDIENCE is missing.");

// -----------------------
// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dbUrl));

// -----------------------
// Configure Identity
builder.Services.AddIdentity<AppUser, Microsoft.AspNetCore.Identity.IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// -----------------------
// Configure JWT Authentication
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);// decode Base64 to bytes
var key = new SymmetricSecurityKey(keyBytes);
var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

// -----------------------
// Authorization
builder.Services.AddAuthorization();

// -----------------------
// Controllers & CORS
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
        policy => policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// -----------------------
// Test database connection
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        if (await dbContext.Database.CanConnectAsync())
            Console.WriteLine("✅ Database connected successfully!");
        else
            Console.WriteLine("⚠️ Database connection failed!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database connection exception: {ex.Message}");
    }
}

// -----------------------
// Middleware
app.UseCors("AllowReact");
app.UseAuthentication();
app.UseAuthorization();

// -----------------------
// Map Controllers
app.MapControllers();

app.Run();

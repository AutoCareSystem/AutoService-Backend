using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using AutoServiceBackend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using backend_EAD.Models;
using backend_EAD.Services;

var builder = WebApplication.CreateBuilder(args);

// -----------------------
// Load .env (for local development)
DotNetEnv.Env.Load();
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
// Register HttpClient and NotificationHelper
builder.Services.AddHttpClient();
builder.Services.AddScoped<NotificationHelper>();

// -----------------------
// Configure JWT Authentication
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
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
// Authorization, Controllers, CORS
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// -----------------------
// Swagger (must be after builder)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Authentication Service API",
        Version = "v1",
        Description = "API for authentication and user profile management"
    });

    // Add JWT Authentication
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your JWT token. Example: 'Bearer eyJhbGc...'"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// -----------------------
// Swagger UI visible everywhere (including Docker)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API V1");
    c.RoutePrefix = string.Empty; // open at root "/"
});

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

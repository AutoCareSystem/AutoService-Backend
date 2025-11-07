using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend_EAD.Models;
using AutoServiceBackend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(UserManager<AppUser> userManager, AppDbContext db, IConfiguration config)
    {
        _userManager = userManager;
        _db = db;
        _config = config;
    }

    // -------------------------------
    // REGISTER
    // -------------------------------
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = new AppUser { UserName = dto.Email, Email = dto.Email, Role = dto.Role };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        // Create Customer or Employee record based on role
        if (dto.Role == "Customer")
        {
            var customer = new Customer { UserID = user.Id, LoyaltyPoints = 0 };
            _db.Customers.Add(customer);
        }
        else if (dto.Role == "Employee")
        {
            var employee = new Employee { UserID = user.Id, IsActive = true, Position = "Staff" };
            _db.Employees.Add(employee);
        }

        await _db.SaveChangesAsync();
        return Ok("User registered successfully");
    }

    // -------------------------------
    // LOGIN
    // -------------------------------
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) return Unauthorized("Invalid credentials");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid) return Unauthorized("Invalid credentials");

        // Generate tokens
        var accessToken = GenerateJwtToken(user);
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        _db.RefreshTokens.Add(refreshToken);

        // Get related IDs
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.UserID == user.Id);
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.UserID == user.Id);

        string? vehicleID = null;
        if (customer != null)
        {
            vehicleID = await _db.Vehicles
                .Where(v => v.Customer.UserID == customer.UserID)
                .Select(v => v.VehicleID.ToString())
                .FirstOrDefaultAsync();
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            accessToken,
            refreshToken = refreshToken.Token,
            role = user.Role.ToLower(),
            userId = user.Id,
            customerID = customer?.UserID,
            employeeID = employee?.UserID,
            vehicleID,
            email = user.Email
        });
    }

    // -------------------------------
    // REFRESH TOKEN
    // -------------------------------
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest dto)
    {
        var tokenEntity = _db.RefreshTokens.FirstOrDefault(r => r.Token == dto.RefreshToken && !r.Revoked);
        if (tokenEntity == null || tokenEntity.Expires < DateTime.UtcNow)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(tokenEntity.UserId);
        if (user == null)
            return BadRequest("User authentication failed");

        var token = GenerateJwtToken(user);
        return Ok(new { Token = token });
    }

    // -------------------------------
    // JWT GENERATION
    // -------------------------------
    private string GenerateJwtToken(AppUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.Email, user.Email  ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// -------------------------------
// DTOs
// -------------------------------
public record RegisterDto(string Email, string Password, string Role);
public record LoginDto(string Email, string Password);
public record RefreshRequest(string RefreshToken);

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
            await _db.SaveChangesAsync();
        }
        else if (dto.Role == "Employee")
        {
            // Auto-generate EmpNo
            var lastEmployee = await _db.Employees
                .OrderByDescending(e => e.EmpNo)
                .FirstOrDefaultAsync();

            string newEmpNo;
            if (lastEmployee == null || string.IsNullOrEmpty(lastEmployee.EmpNo))
            {
                // If no employees exist, start with EMP001
                newEmpNo = "EMP001";
            }
            else
            {
                // Extract the numeric part and increment
                var lastNumber = int.Parse(lastEmployee.EmpNo.Substring(3));
                newEmpNo = $"EMP{(lastNumber + 1):D3}";
            }

            var employee = new Employee
            {
                UserID = user.Id,
                IsActive = true,
                Position = "Staff",
                EmpNo = newEmpNo
            };
            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();
        }

        return Ok("User registered");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized();

        var accessToken = GenerateJwtToken(user);

        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7),
            Revoked = false
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return Ok(new { accessToken, refreshToken = refreshToken.Token, role = user.Role });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest dto)
    {
        var tokenEntity = _db.RefreshTokens.FirstOrDefault(r => r.Token == dto.RefreshToken && !r.Revoked);
        if (tokenEntity == null || tokenEntity.Expires < DateTime.UtcNow) return Unauthorized();

        var user = await _userManager.FindByIdAsync(tokenEntity.UserId);
        // Before calling GenerateJwtToken:
        if (user != null)
        {
            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }
        else
        {
            return BadRequest("User authentication failed");
        }
    }

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

public record RegisterDto(string Email, string Password, string Role);
public record LoginDto(string Email, string Password);
public record RefreshRequest(string RefreshToken);
